#nullable enable
using System;
using System.Collections.Generic;
using Routya.ResultKit.Builders;

namespace Routya.ResultKit
{
    /// <summary>
    /// Represents the result of an operation that can either succeed with data or fail with RFC 7807 compliant problem details.
    /// </summary>
    /// <typeparam name="T">The type of data returned on success.</typeparam>
    public class Result<T>
    {
        /// <summary>
        /// Indicates whether the operation succeeded.
        /// </summary>
        public bool Success { get; }
        
        /// <summary>
        /// The data returned by the operation on success.
        /// </summary>
        public T Data { get; }
        
        /// <summary>
        /// RFC 7807 compliant problem details if the operation failed.
        /// </summary>
        public ProblemDetails? Error { get; }
        
        /// <summary>
        /// The HTTP status code associated with this result (e.g., 200 for Ok, 201 for Created, 202 for Accepted, or from ProblemDetails for failures).
        /// </summary>
        public int StatusCode { get; }
        
        /// <summary>
        /// The redirect location URI for redirect results (301, 302).
        /// </summary>
        public string? RedirectLocation { get; }

        private Result(bool success, T data, ProblemDetails? error, int statusCode = 200, string? redirectLocation = null)
        {
            Success = success;
            Data = data;
            Error = error;
            StatusCode = statusCode;
            RedirectLocation = redirectLocation;
        }

        /// <summary>
        /// Creates a successful result with the provided data.
        /// </summary>
        /// <param name="data">The data to return.</param>
        /// <returns>A successful Result instance.</returns>
        public static Result<T> Ok(T data) => new Result<T>(true, data, null, 200, null);

        /// <summary>
        /// Creates a successful result with the provided data for a Created (201) response.
        /// </summary>
        /// <param name="data">The data to return.</param>
        /// <returns>A successful Result instance with 201 status code.</returns>
        public static Result<T> Created(T data) => new Result<T>(true, data, null, 201, null);

        /// <summary>
        /// Creates a successful result with the provided data for an Accepted (202) response.
        /// </summary>
        /// <param name="data">The data to return.</param>
        /// <returns>A successful Result instance with 202 status code.</returns>
        public static Result<T> Accepted(T data) => new Result<T>(true, data, null, 202, null);

        /// <summary>
        /// Creates a successful result with no content for a No Content (204) response.
        /// Typically used for DELETE or PUT operations that succeed without returning data.
        /// </summary>
        /// <returns>A successful Result instance with 204 status code.</returns>
        public static Result<T> NoContent() => new Result<T>(true, default!, null, 204, null);

        /// <summary>
        /// Creates a successful redirect result with the specified location.
        /// </summary>
        /// <param name="location">The URI to redirect to.</param>
        /// <param name="permanent">Whether the redirect is permanent (301) or temporary (302). Default is temporary.</param>
        /// <returns>A successful Result instance with 301 or 302 status code.</returns>
        public static Result<T> Redirect(string location, bool permanent = false)
        {
            if (string.IsNullOrWhiteSpace(location))
                throw new ArgumentNullException(nameof(location), "Redirect location cannot be null or empty.");
            
            return new Result<T>(true, default!, null, permanent ? 301 : 302, location);
        }

        /// <summary>
        /// Creates a successful permanent redirect result with the specified location.
        /// </summary>
        /// <param name="location">The URI to redirect to.</param>
        /// <returns>A successful Result instance with 301 status code.</returns>
        public static Result<T> RedirectPermanent(string location) => Redirect(location, permanent: true);

        /// <summary>
        /// Creates a failed result with the provided ProblemDetails.
        /// </summary>
        /// <param name="problem">The RFC 7807 compliant problem details.</param>
        /// <returns>A failed Result instance.</returns>
        public static Result<T> Fail(ProblemDetails problem) => new Result<T>(false, default!, problem, problem.Status ?? 500, null);

        /// <summary>
        /// Creates a failed result with the provided title, status code, and optional errors.
        /// </summary>
        /// <param name="title">The problem title.</param>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <param name="errors">Optional validation errors dictionary.</param>
        /// <returns>A failed Result instance.</returns>
        [Obsolete("Use specific factory methods like ValidationFailed() or create ProblemDetails with ProblemDetailsBuilder. See migration guide at https://github.com/HBartosch/Routya.ResultKit/blob/main/docs/MIGRATION_V2.md. This method will be removed in v3.0.", false)]
        public static Result<T> Fail(string title, int statusCode, IDictionary<string, string[]>? errors = null)
        {
            var problem = new ProblemDetails
            {
                Title = title,
                Status = statusCode
            };
            
            if (errors != null)
            {
                problem.SetExtension(Constants.ErrorDictionaryKey, errors);
            }
            
            return new Result<T>(false, default!, problem);
        }

        // RFC 7807 Compliant Factory Methods

        /// <summary>
        /// Creates a failed result with a Bad Request (400) problem.
        /// </summary>
        /// <param name="detail">A human-readable explanation of the problem.</param>
        /// <param name="instance">Optional URI reference identifying this specific occurrence.</param>
        /// <returns>A failed Result instance.</returns>
        public static Result<T> BadRequest(string detail, string? instance = null)
        {
            var problem = ProblemDetailsBuilder.BadRequest(detail)
                .WithInstance(instance ?? string.Empty)
                .Build();
            return Fail(problem);
        }

        /// <summary>
        /// Creates a failed result with a Not Found (404) problem.
        /// </summary>
        /// <param name="detail">A human-readable explanation of the problem.</param>
        /// <param name="instance">Optional URI reference identifying this specific occurrence.</param>
        /// <returns>A failed Result instance.</returns>
        public static Result<T> NotFound(string detail, string? instance = null)
        {
            var problem = ProblemDetailsBuilder.NotFound(detail)
                .WithInstance(instance ?? string.Empty)
                .Build();
            return Fail(problem);
        }

        /// <summary>
        /// Creates a failed result with a Validation Failed (400) problem.
        /// </summary>
        /// <param name="errors">Dictionary of field names to error messages.</param>
        /// <param name="instance">Optional URI reference identifying this specific occurrence.</param>
        /// <returns>A failed Result instance.</returns>
        public static Result<T> ValidationFailed(IDictionary<string, string[]> errors, string? instance = null)
        {
            var problem = ProblemDetailsBuilder.ValidationError(errors)
                .WithInstance(instance ?? string.Empty)
                .Build();
            return Fail(problem);
        }

        /// <summary>
        /// Creates a failed result with an Unauthorized (401) problem.
        /// </summary>
        /// <param name="detail">A human-readable explanation of the problem.</param>
        /// <param name="instance">Optional URI reference identifying this specific occurrence.</param>
        /// <returns>A failed Result instance.</returns>
        public static Result<T> Unauthorized(string detail, string? instance = null)
        {
            var problem = ProblemDetailsBuilder.Unauthorized(detail)
                .WithInstance(instance ?? string.Empty)
                .Build();
            return Fail(problem);
        }

        /// <summary>
        /// Creates a failed result with a Forbidden (403) problem.
        /// </summary>
        /// <param name="detail">A human-readable explanation of the problem.</param>
        /// <param name="instance">Optional URI reference identifying this specific occurrence.</param>
        /// <returns>A failed Result instance.</returns>
        public static Result<T> Forbidden(string detail, string? instance = null)
        {
            var problem = ProblemDetailsBuilder.Forbidden(detail)
                .WithInstance(instance ?? string.Empty)
                .Build();
            return Fail(problem);
        }

        /// <summary>
        /// Creates a failed result with a Conflict (409) problem.
        /// </summary>
        /// <param name="detail">A human-readable explanation of the problem.</param>
        /// <param name="instance">Optional URI reference identifying this specific occurrence.</param>
        /// <returns>A failed Result instance.</returns>
        public static Result<T> Conflict(string detail, string? instance = null)
        {
            var problem = ProblemDetailsBuilder.Conflict(detail)
                .WithInstance(instance ?? string.Empty)
                .Build();
            return Fail(problem);
        }

        /// <summary>
        /// Creates a failed result with an Internal Server Error (500) problem.
        /// </summary>
        /// <param name="detail">A human-readable explanation of the problem.</param>
        /// <param name="instance">Optional URI reference identifying this specific occurrence.</param>
        /// <returns>A failed Result instance.</returns>
        public static Result<T> InternalServerError(string detail, string? instance = null)
        {
            var problem = ProblemDetailsBuilder.InternalServerError(detail)
                .WithInstance(instance ?? string.Empty)
                .Build();
            return Fail(problem);
        }
    }
}