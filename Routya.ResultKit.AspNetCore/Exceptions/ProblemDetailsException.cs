using System;
using System.Collections.Generic;

namespace Routya.ResultKit.AspNetCore.Exceptions
{
    /// <summary>
    /// Base exception class that carries RFC 7807 compliant problem details information.
    /// </summary>
    public class ProblemDetailsException : Exception
    {
        /// <summary>
        /// Gets the problem type URI.
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Gets the problem title.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets the HTTP status code.
        /// </summary>
        public int Status { get; }

        /// <summary>
        /// Gets the problem detail message.
        /// </summary>
        public string Detail { get; }

        /// <summary>
        /// Gets the instance URI.
        /// </summary>
        public string? Instance { get; set; }

        /// <summary>
        /// Gets the extension members dictionary.
        /// </summary>
        public Dictionary<string, object?> Extensions { get; } = new Dictionary<string, object?>();

        /// <summary>
        /// Initializes a new instance of the ProblemDetailsException class.
        /// </summary>
        /// <param name="type">The problem type URI.</param>
        /// <param name="title">The problem title.</param>
        /// <param name="status">The HTTP status code.</param>
        /// <param name="detail">The problem detail message.</param>
        /// <param name="instance">Optional instance URI.</param>
        public ProblemDetailsException(
            string type,
            string title,
            int status,
            string detail,
            string? instance = null)
            : base(detail)
        {
            Type = type;
            Title = title;
            Status = status;
            Detail = detail;
            Instance = instance;
        }

        /// <summary>
        /// Initializes a new instance of the ProblemDetailsException class with an inner exception.
        /// </summary>
        /// <param name="type">The problem type URI.</param>
        /// <param name="title">The problem title.</param>
        /// <param name="status">The HTTP status code.</param>
        /// <param name="detail">The problem detail message.</param>
        /// <param name="innerException">The inner exception.</param>
        /// <param name="instance">Optional instance URI.</param>
        public ProblemDetailsException(
            string type,
            string title,
            int status,
            string detail,
            Exception innerException,
            string? instance = null)
            : base(detail, innerException)
        {
            Type = type;
            Title = title;
            Status = status;
            Detail = detail;
            Instance = instance;
        }

        /// <summary>
        /// Converts the exception to a ProblemDetails instance.
        /// </summary>
        /// <returns>A ProblemDetails instance representing this exception.</returns>
        public virtual Routya.ResultKit.ProblemDetails ToProblemDetails()
        {
            var problemDetails = new Routya.ResultKit.ProblemDetails
            {
                Type = Type,
                Title = Title,
                Status = Status,
                Detail = Detail,
                Instance = Instance ?? string.Empty
            };

            foreach (var extension in Extensions)
            {
                problemDetails.SetExtension(extension.Key, extension.Value);
            }

            return problemDetails;
        }
    }
}
