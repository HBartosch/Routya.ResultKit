#nullable enable
using System;
using System.Collections.Generic;
using Routya.ResultKit.ProblemTypes;

namespace Routya.ResultKit.Builders
{
    /// <summary>
    /// Fluent builder for creating RFC 7807 compliant ProblemDetails instances with validation.
    /// </summary>
    public class ProblemDetailsBuilder
    {
        private string _type = "about:blank";
        private string? _title;
        private int? _status;
        private string? _detail;
        private string? _instance;
        private readonly Dictionary<string, object?> _extensions = new Dictionary<string, object?>();

        /// <summary>
        /// Sets the problem type URI. Must be a valid URI or URN.
        /// </summary>
        /// <param name="type">The type URI (e.g., "https://example.com/problems/validation" or "urn:problem-type:validation-error").</param>
        /// <returns>The builder instance for method chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when the type is not a valid URI or URN.</exception>
        public ProblemDetailsBuilder WithType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new ArgumentException("Type cannot be null or whitespace.", nameof(type));
            }

            if (!Uri.TryCreate(type, UriKind.Absolute, out _) && !IsValidUrn(type))
            {
                throw new ArgumentException($"Type '{type}' must be a valid absolute URI or URN.", nameof(type));
            }

            _type = type;
            return this;
        }

        /// <summary>
        /// Sets the problem title.
        /// </summary>
        /// <param name="title">A short, human-readable summary of the problem.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public ProblemDetailsBuilder WithTitle(string title)
        {
            _title = title;
            return this;
        }

        /// <summary>
        /// Sets the HTTP status code. Must be a valid HTTP status code (100-599).
        /// </summary>
        /// <param name="status">The HTTP status code.</param>
        /// <returns>The builder instance for method chaining.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the status code is not in the valid range (100-599).</exception>
        public ProblemDetailsBuilder WithStatus(int status)
        {
            if (status < 100 || status > 599)
            {
                throw new ArgumentOutOfRangeException(nameof(status), status, "HTTP status code must be between 100 and 599.");
            }

            _status = status;
            return this;
        }

        /// <summary>
        /// Sets the problem detail message.
        /// </summary>
        /// <param name="detail">A human-readable explanation specific to this occurrence of the problem.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public ProblemDetailsBuilder WithDetail(string detail)
        {
            _detail = detail;
            return this;
        }

        /// <summary>
        /// Sets the instance URI. Must be a valid URI.
        /// </summary>
        /// <param name="instance">A URI reference identifying the specific occurrence of the problem.</param>
        /// <returns>The builder instance for method chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when the instance is not a valid URI.</exception>
        public ProblemDetailsBuilder WithInstance(string instance)
        {
            if (!string.IsNullOrWhiteSpace(instance))
            {
                if (!Uri.TryCreate(instance, UriKind.RelativeOrAbsolute, out var uri))
                {
                    throw new ArgumentException($"Instance '{instance}' must be a valid URI.", nameof(instance));
                }

                // Additional validation: URIs should not contain unescaped spaces
                if (instance.Contains(" "))
                {
                    throw new ArgumentException($"Instance '{instance}' must be a valid URI.", nameof(instance));
                }
            }

            _instance = instance;
            return this;
        }

        /// <summary>
        /// Adds an extension member to be serialized as a top-level property.
        /// </summary>
        /// <typeparam name="T">The type of the extension value.</typeparam>
        /// <param name="key">The key for the extension member.</param>
        /// <param name="value">The value to store.</param>
        /// <returns>The builder instance for method chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when the key is invalid or conflicts with RFC 7807 properties.</exception>
        public ProblemDetailsBuilder WithExtension<T>(string key, T value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Extension key cannot be null or whitespace.", nameof(key));
            }

            var lowerKey = key.ToLowerInvariant();
            if (lowerKey == "type" || lowerKey == "title" || lowerKey == "status" || 
                lowerKey == "detail" || lowerKey == "instance")
            {
                throw new ArgumentException($"Extension key '{key}' conflicts with RFC 7807 property names.", nameof(key));
            }

            _extensions[key] = value;
            return this;
        }

        /// <summary>
        /// Builds the ProblemDetails instance.
        /// </summary>
        /// <returns>An immutable ProblemDetails instance.</returns>
        public ProblemDetails Build()
        {
            var problemDetails = new ProblemDetails
            {
                Type = _type,
                Title = _title,
                Status = _status,
                Detail = _detail,
                Instance = _instance
            };

            foreach (var extension in _extensions)
            {
                problemDetails.SetExtension(extension.Key, extension.Value);
            }

            return problemDetails;
        }

        // Static factory methods for common problem types

        /// <summary>
        /// Creates a builder for a Created (201) response.
        /// Note: This is not typically used for errors, but provided for completeness.
        /// </summary>
        /// <param name="detail">Optional detail message.</param>
        /// <returns>A new ProblemDetailsBuilder instance.</returns>
        public static ProblemDetailsBuilder Created(string? detail = null)
        {
            return new ProblemDetailsBuilder()
                .WithType(StandardProblemTypes.Created)
                .WithTitle("Created")
                .WithStatus(201)
                .WithDetail(detail ?? string.Empty);
        }

        /// <summary>
        /// Creates a builder for an Accepted (202) response.
        /// Note: This is not typically used for errors, but provided for completeness.
        /// </summary>
        /// <param name="detail">Optional detail message.</param>
        /// <returns>A new ProblemDetailsBuilder instance.</returns>
        public static ProblemDetailsBuilder Accepted(string? detail = null)
        {
            return new ProblemDetailsBuilder()
                .WithType(StandardProblemTypes.Accepted)
                .WithTitle("Accepted")
                .WithStatus(202)
                .WithDetail(detail ?? string.Empty);
        }

        /// <summary>
        /// Creates a builder for a Bad Request (400) problem.
        /// </summary>
        /// <param name="detail">Optional detail message.</param>
        /// <returns>A new ProblemDetailsBuilder instance.</returns>
        public static ProblemDetailsBuilder BadRequest(string? detail = null)
        {
            return new ProblemDetailsBuilder()
                .WithType(StandardProblemTypes.BadRequest)
                .WithTitle("Bad Request")
                .WithStatus(400)
                .WithDetail(detail ?? "The request is invalid.");
        }

        /// <summary>
        /// Creates a builder for an Unauthorized (401) problem.
        /// </summary>
        /// <param name="detail">Optional detail message.</param>
        /// <returns>A configured ProblemDetailsBuilder.</returns>
        public static ProblemDetailsBuilder Unauthorized(string? detail = null)
        {
            return new ProblemDetailsBuilder()
                .WithType(StandardProblemTypes.Unauthorized)
                .WithTitle("Unauthorized")
                .WithStatus(401)
                .WithDetail(detail ?? "Authentication is required.");
        }

        /// <summary>
        /// Creates a builder for a Forbidden (403) problem.
        /// </summary>
        /// <param name="detail">Optional detail message.</param>
        /// <returns>A configured ProblemDetailsBuilder.</returns>
        public static ProblemDetailsBuilder Forbidden(string? detail = null)
        {
            return new ProblemDetailsBuilder()
                .WithType(StandardProblemTypes.Forbidden)
                .WithTitle("Forbidden")
                .WithStatus(403)
                .WithDetail(detail ?? "You do not have permission to access this resource.");
        }

        /// <summary>
        /// Creates a builder for a Not Found (404) problem.
        /// </summary>
        /// <param name="detail">Optional detail message.</param>
        /// <returns>A configured ProblemDetailsBuilder.</returns>
        public static ProblemDetailsBuilder NotFound(string? detail = null)
        {
            return new ProblemDetailsBuilder()
                .WithType(StandardProblemTypes.NotFound)
                .WithTitle("Not Found")
                .WithStatus(404)
                .WithDetail(detail ?? "The requested resource was not found.");
        }

        /// <summary>
        /// Creates a builder for a Conflict (409) problem.
        /// </summary>
        /// <param name="detail">Optional detail message.</param>
        /// <returns>A configured ProblemDetailsBuilder.</returns>
        public static ProblemDetailsBuilder Conflict(string? detail = null)
        {
            return new ProblemDetailsBuilder()
                .WithType(StandardProblemTypes.Conflict)
                .WithTitle("Conflict")
                .WithStatus(409)
                .WithDetail(detail ?? "The request conflicts with the current state of the resource.");
        }

        /// <summary>
        /// Creates a builder for a Validation Error problem.
        /// </summary>
        /// <param name="errors">Optional validation errors dictionary.</param>
        /// <returns>A configured ProblemDetailsBuilder.</returns>
        public static ProblemDetailsBuilder ValidationError(IDictionary<string, string[]>? errors = null)
        {
            var builder = new ProblemDetailsBuilder()
                .WithType(StandardProblemTypes.ValidationError)
                .WithTitle("Validation Failed")
                .WithStatus(400)
                .WithDetail("One or more validation errors occurred.");

            if (errors != null)
            {
                builder.WithExtension("errors", errors);
            }

            return builder;
        }

        /// <summary>
        /// Creates a builder for an Internal Server Error (500) problem.
        /// </summary>
        /// <param name="detail">Optional detail message.</param>
        /// <returns>A configured ProblemDetailsBuilder.</returns>
        public static ProblemDetailsBuilder InternalServerError(string? detail = null)
        {
            return new ProblemDetailsBuilder()
                .WithType(StandardProblemTypes.InternalServerError)
                .WithTitle("Internal Server Error")
                .WithStatus(500)
                .WithDetail(detail ?? "An unexpected error occurred while processing the request.");
        }

        private static bool IsValidUrn(string value)
        {
            // Basic URN validation: must start with "urn:" followed by NID:NSS
            return value.StartsWith("urn:", StringComparison.OrdinalIgnoreCase) && 
                   value.Length > 4 && 
                   value.IndexOf(':', 4) > 0;
        }
    }
}
