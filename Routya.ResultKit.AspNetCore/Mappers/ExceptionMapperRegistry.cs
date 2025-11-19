using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Routya.ResultKit.AspNetCore.Exceptions;
using Routya.ResultKit.Builders;
using Routya.ResultKit.ProblemTypes;

namespace Routya.ResultKit.AspNetCore.Mappers
{
    /// <summary>
    /// Registry for managing exception-to-ProblemDetails mappers.
    /// </summary>
    public class ExceptionMapperRegistry
    {
        private readonly List<IExceptionMapper> _mappers = new List<IExceptionMapper>();

        /// <summary>
        /// Initializes a new instance with default mappers.
        /// </summary>
        public ExceptionMapperRegistry()
        {
            // Register default mappers
            Register(new ProblemDetailsExceptionMapper());
            Register(new ArgumentExceptionMapper());
            Register(new UnauthorizedAccessExceptionMapper());
            Register(new InvalidOperationExceptionMapper());
            Register(new NotImplementedExceptionMapper());
        }

        /// <summary>
        /// Registers a custom exception mapper.
        /// </summary>
        /// <param name="mapper">The mapper to register.</param>
        public void Register(IExceptionMapper mapper)
        {
            _mappers.Insert(0, mapper); // Insert at beginning for custom overrides
        }

        /// <summary>
        /// Maps an exception to ProblemDetails using registered mappers.
        /// </summary>
        /// <param name="exception">The exception to map.</param>
        /// <param name="context">The HTTP context.</param>
        /// <returns>A ProblemDetails instance.</returns>
        public Routya.ResultKit.ProblemDetails Map(Exception exception, HttpContext context)
        {
            var mapper = _mappers.FirstOrDefault(m => m.CanHandle(exception));
            
            if (mapper != null)
            {
                return mapper.Map(exception, context);
            }

            // Fallback to generic internal server error
            return ProblemDetailsBuilder.InternalServerError(exception.Message)
                .WithInstance(context.Request.Path.Value ?? string.Empty)
                .Build();
        }
    }

    /// <summary>
    /// Mapper for ProblemDetailsException.
    /// </summary>
    internal class ProblemDetailsExceptionMapper : IExceptionMapper
    {
        public bool CanHandle(Exception exception) => exception is ProblemDetailsException;

        public Routya.ResultKit.ProblemDetails Map(Exception exception, HttpContext context)
        {
            var problemException = (ProblemDetailsException)exception;
            var problemDetails = problemException.ToProblemDetails();
            
            // Set instance if not already set
            if (string.IsNullOrEmpty(problemDetails.Instance))
            {
                var builder = new ProblemDetailsBuilder()
                    .WithType(problemDetails.Type)
                    .WithTitle(problemDetails.Title!)
                    .WithStatus(problemDetails.Status!.Value)
                    .WithDetail(problemDetails.Detail)
                    .WithInstance(context.Request.Path.Value ?? string.Empty);

                foreach (var ext in problemDetails.GetExtensions())
                {
                    builder.WithExtension(ext.Key, ext.Value);
                }

                return builder.Build();
            }

            return problemDetails;
        }
    }

    /// <summary>
    /// Mapper for ArgumentException and related exceptions.
    /// </summary>
    internal class ArgumentExceptionMapper : IExceptionMapper
    {
        public bool CanHandle(Exception exception) =>
            exception is ArgumentException ||
            exception is ArgumentNullException ||
            exception is ArgumentOutOfRangeException;

        public Routya.ResultKit.ProblemDetails Map(Exception exception, HttpContext context)
        {
            return ProblemDetailsBuilder.BadRequest(exception.Message)
                .WithInstance(context.Request.Path.Value ?? string.Empty)
                .Build();
        }
    }

    /// <summary>
    /// Mapper for UnauthorizedAccessException.
    /// </summary>
    internal class UnauthorizedAccessExceptionMapper : IExceptionMapper
    {
        public bool CanHandle(Exception exception) => exception is UnauthorizedAccessException;

        public Routya.ResultKit.ProblemDetails Map(Exception exception, HttpContext context)
        {
            return ProblemDetailsBuilder.Forbidden(exception.Message)
                .WithInstance(context.Request.Path.Value ?? string.Empty)
                .Build();
        }
    }

    /// <summary>
    /// Mapper for InvalidOperationException.
    /// </summary>
    internal class InvalidOperationExceptionMapper : IExceptionMapper
    {
        public bool CanHandle(Exception exception) => exception is InvalidOperationException;

        public Routya.ResultKit.ProblemDetails Map(Exception exception, HttpContext context)
        {
            return ProblemDetailsBuilder.Conflict(exception.Message)
                .WithInstance(context.Request.Path.Value ?? string.Empty)
                .Build();
        }
    }

    /// <summary>
    /// Mapper for NotImplementedException.
    /// </summary>
    internal class NotImplementedExceptionMapper : IExceptionMapper
    {
        public bool CanHandle(Exception exception) => exception is NotImplementedException;

        public Routya.ResultKit.ProblemDetails Map(Exception exception, HttpContext context)
        {
            return new ProblemDetailsBuilder()
                .WithType(StandardProblemTypes.Custom("not-implemented"))
                .WithTitle("Not Implemented")
                .WithStatus(501)
                .WithDetail(exception.Message)
                .WithInstance(context.Request.Path.Value ?? string.Empty)
                .Build();
        }
    }
}
