using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Routya.ResultKit.Serialization
{
    /// <summary>
    /// Custom JSON converter for ProblemDetails that serializes RFC 7807 properties and extension members as top-level JSON properties.
    /// Uses caching for improved performance.
    /// </summary>
    public class ProblemDetailsJsonConverter : JsonConverter<ProblemDetails>
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new ConcurrentDictionary<Type, PropertyInfo[]>();
        
        private static readonly HashSet<string> Rfc7807Properties = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "type", "title", "status", "detail", "instance"
        };

        public override ProblemDetails Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected StartObject token");
            }

            string? type = "about:blank";
            string? title = null;
            int? status = null;
            string? detail = null;
            string? instance = null;
            var extensions = new Dictionary<string, object?>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException("Expected PropertyName token");
                }

                var propertyName = reader.GetString();
                reader.Read();

                var propertyNameLower = propertyName?.ToLowerInvariant();

                switch (propertyNameLower)
                {
                    case "type":
                        type = reader.GetString();
                        break;
                    case "title":
                        title = reader.GetString();
                        break;
                    case "status":
                        status = reader.GetInt32();
                        break;
                    case "detail":
                        detail = reader.GetString();
                        break;
                    case "instance":
                        instance = reader.GetString();
                        break;
                    default:
                        // Extension member - deserialize as JsonElement for flexibility
                        var element = JsonSerializer.Deserialize<JsonElement>(ref reader, options);
                        extensions[propertyName!] = element;
                        break;
                }
            }

            // Create ProblemDetails using reflection since init properties can't be set after construction
            var problemDetails = new ProblemDetails
            {
                Type = type ?? "about:blank",
                Title = title,
                Status = status,
                Detail = detail,
                Instance = instance
            };

            // Set extensions
            foreach (var kvp in extensions)
            {
                problemDetails.SetExtension(kvp.Key, kvp.Value);
            }

            return problemDetails;
        }

        public override void Write(Utf8JsonWriter writer, ProblemDetails value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            // Get naming policy (default to camelCase)
            var namingPolicy = options.PropertyNamingPolicy ?? JsonNamingPolicy.CamelCase;

            // Write RFC 7807 core properties
            writer.WriteString(namingPolicy.ConvertName("Type"), value.Type);

            if (value.Title != null)
            {
                writer.WriteString(namingPolicy.ConvertName("Title"), value.Title);
            }

            if (value.Status.HasValue)
            {
                writer.WriteNumber(namingPolicy.ConvertName("Status"), value.Status.Value);
            }

            if (value.Detail != null)
            {
                writer.WriteString(namingPolicy.ConvertName("Detail"), value.Detail);
            }

            if (value.Instance != null)
            {
                writer.WriteString(namingPolicy.ConvertName("Instance"), value.Instance);
            }

            // Write extension members as top-level properties
            var extensions = value.GetExtensions();
            foreach (var kvp in extensions)
            {
                var extensionName = namingPolicy.ConvertName(kvp.Key);
                
                // Ensure extension names don't conflict with RFC 7807 properties
                if (Rfc7807Properties.Contains(extensionName))
                {
                    throw new JsonException($"Extension member '{kvp.Key}' conflicts with RFC 7807 property name. Use a different key.");
                }

                writer.WritePropertyName(extensionName);
                JsonSerializer.Serialize(writer, kvp.Value, kvp.Value?.GetType() ?? typeof(object), options);
            }

            writer.WriteEndObject();
        }
    }
}
