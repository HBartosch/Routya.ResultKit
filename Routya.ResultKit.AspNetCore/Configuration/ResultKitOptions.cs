using System.Text.Json;
using Routya.ResultKit.ProblemTypes;

namespace Routya.ResultKit.AspNetCore.Configuration
{
    /// <summary>
    /// Configuration options for ResultKit ASP.NET Core integration.
    /// </summary>
    public class ResultKitOptions
    {
        /// <summary>
        /// Gets or sets the base URI for domain-specific problem types.
        /// Default is "urn:problem-type:".
        /// </summary>
        public string ProblemTypeBaseUri { get; set; } = "urn:problem-type:";

        /// <summary>
        /// Gets or sets whether to include exception details in problem responses.
        /// Default is false. Should be true only in development environments.
        /// </summary>
        public bool IncludeExceptionDetails { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to include trace ID as an extension member.
        /// Default is false. Set to true to automatically add trace IDs to all problem responses.
        /// </summary>
        public bool IncludeTraceId { get; set; } = false;

        /// <summary>
        /// Gets or sets the name of the trace ID extension member.
        /// Default is "traceId".
        /// </summary>
        public string TraceIdExtensionName { get; set; } = "traceId";

        /// <summary>
        /// Gets or sets the JSON naming policy for serializing ProblemDetails.
        /// Default is camelCase.
        /// </summary>
        public JsonNamingPolicy NamingPolicy { get; set; } = JsonNamingPolicy.CamelCase;

        /// <summary>
        /// Applies the configured base URI to StandardProblemTypes.
        /// </summary>
        internal void ApplyToStandardProblemTypes()
        {
            StandardProblemTypes.DefaultBaseUri = ProblemTypeBaseUri;
        }
    }
}
