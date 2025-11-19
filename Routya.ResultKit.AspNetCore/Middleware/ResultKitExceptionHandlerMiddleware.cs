using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Routya.ResultKit.AspNetCore.Configuration;
using Routya.ResultKit.AspNetCore.Converters;
using Routya.ResultKit.AspNetCore.Mappers;

namespace Routya.ResultKit.AspNetCore.Middleware
{
    /// <summary>
    /// Middleware for handling exceptions and converting them to RFC 7807 compliant ProblemDetails responses.
    /// </summary>
    public class ResultKitExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ResultKitExceptionHandlerMiddleware> _logger;
        private readonly ResultKitOptions _options;
        private readonly ExceptionMapperRegistry _mapperRegistry;

        /// <summary>
        /// Initializes a new instance of the ResultKitExceptionHandlerMiddleware class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="options">The ResultKit options.</param>
        /// <param name="mapperRegistry">The exception mapper registry.</param>
        public ResultKitExceptionHandlerMiddleware(
            RequestDelegate next,
            ILogger<ResultKitExceptionHandlerMiddleware> _logger,
            IOptions<ResultKitOptions> options,
            ExceptionMapperRegistry mapperRegistry)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            this._logger = _logger ?? throw new ArgumentNullException(nameof(_logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _mapperRegistry = mapperRegistry ?? throw new ArgumentNullException(nameof(mapperRegistry));
        }

        /// <summary>
        /// Invokes the middleware.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Map exception to ProblemDetails
            var problemDetails = _mapperRegistry.Map(exception, context);

            // Add trace ID if configured
            if (_options.IncludeTraceId)
            {
                var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
                problemDetails.SetExtension(_options.TraceIdExtensionName, traceId);
            }

            // Add exception details if configured (typically for development)
            if (_options.IncludeExceptionDetails)
            {
                problemDetails.SetExtension("exceptionType", exception.GetType().FullName);
                problemDetails.SetExtension("exceptionMessage", exception.Message);
                
                if (!string.IsNullOrEmpty(exception.StackTrace))
                {
                    problemDetails.SetExtension("stackTrace", exception.StackTrace);
                }

                if (exception.InnerException != null)
                {
                    problemDetails.SetExtension("innerException", new
                    {
                        type = exception.InnerException.GetType().FullName,
                        message = exception.InnerException.Message
                    });
                }
            }

            // Convert to Microsoft ProblemDetails for serialization
            var microsoftProblemDetails = ProblemDetailsConverter.ToMicrosoft(problemDetails);

            // Set response properties
            context.Response.Clear();
            context.Response.StatusCode = microsoftProblemDetails.Status ?? 500;
            context.Response.ContentType = "application/problem+json";

            // Serialize using configured naming policy
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = _options.NamingPolicy,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            await context.Response.WriteAsJsonAsync(microsoftProblemDetails, options);
        }
    }
}
