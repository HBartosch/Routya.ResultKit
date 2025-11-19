namespace Routya.ResultKit.ProblemTypes
{
    /// <summary>
    /// Provides standard problem type URIs following RFC 9110 for HTTP errors and custom URIs for domain-specific problems.
    /// </summary>
    public static class StandardProblemTypes
    {
        /// <summary>
        /// Default base URI for domain-specific problem types. Can be overridden in configuration.
        /// </summary>
        public static string DefaultBaseUri { get; set; } = "urn:problem-type:";

        // RFC 9110 HTTP Status Codes - https://datatracker.ietf.org/doc/html/rfc9110

        // Success Status Codes

        /// <summary>
        /// Created (201) - The request has been fulfilled and has resulted in one or more new resources being created.
        /// </summary>
        public static string Created => "https://datatracker.ietf.org/doc/html/rfc9110#section-15.3.2";

        /// <summary>
        /// Accepted (202) - The request has been accepted for processing, but the processing has not been completed.
        /// </summary>
        public static string Accepted => "https://datatracker.ietf.org/doc/html/rfc9110#section-15.3.3";

        // Client Error Status Codes

        /// <summary>
        /// Bad Request (400) - The server cannot or will not process the request due to something perceived to be a client error.
        /// </summary>
        public static string BadRequest => "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.1";

        /// <summary>
        /// Unauthorized (401) - The request has not been applied because it lacks valid authentication credentials.
        /// </summary>
        public static string Unauthorized => "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.2";

        /// <summary>
        /// Forbidden (403) - The server understood the request but refuses to authorize it.
        /// </summary>
        public static string Forbidden => "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.4";

        /// <summary>
        /// Not Found (404) - The origin server did not find a current representation for the target resource.
        /// </summary>
        public static string NotFound => "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.5";

        /// <summary>
        /// Conflict (409) - The request could not be completed due to a conflict with the current state of the target resource.
        /// </summary>
        public static string Conflict => "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.10";

        /// <summary>
        /// Unprocessable Entity (422) - The server understands the content type and syntax but was unable to process the contained instructions.
        /// </summary>
        public static string UnprocessableEntity => "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.21";

        /// <summary>
        /// Internal Server Error (500) - The server encountered an unexpected condition that prevented it from fulfilling the request.
        /// </summary>
        public static string InternalServerError => "https://datatracker.ietf.org/doc/html/rfc9110#section-15.6.1";

        // Domain-Specific Problem Types (using configurable base URI)

        /// <summary>
        /// Validation Error - The request failed one or more validation rules.
        /// </summary>
        public static string ValidationError => $"{DefaultBaseUri}validation-error";

        /// <summary>
        /// Business Rule Violation - The request violated a business rule.
        /// </summary>
        public static string BusinessRuleViolation => $"{DefaultBaseUri}business-rule-violation";

        /// <summary>
        /// Resource Already Exists - The resource being created already exists.
        /// </summary>
        public static string ResourceAlreadyExists => $"{DefaultBaseUri}resource-already-exists";

        /// <summary>
        /// Operation Not Permitted - The requested operation is not permitted in the current state.
        /// </summary>
        public static string OperationNotPermitted => $"{DefaultBaseUri}operation-not-permitted";

        /// <summary>
        /// Creates a custom domain-specific problem type URI using the configured base URI.
        /// </summary>
        /// <param name="typeName">The type name (e.g., "user-not-found").</param>
        /// <returns>A complete problem type URI.</returns>
        public static string Custom(string typeName)
        {
            return $"{DefaultBaseUri}{typeName}";
        }
    }
}
