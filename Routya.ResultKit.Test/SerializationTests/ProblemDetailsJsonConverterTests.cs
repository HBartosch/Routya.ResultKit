using System.Text.Json;
using Routya.ResultKit.Serialization;
using Shouldly;

namespace Routya.ResultKit.Test.SerializationTests;

public class ProblemDetailsJsonConverterTests
{
    [Fact]
    public void Serialize_ShouldProduceRfc7807Format()
    {
        var problem = new ProblemDetails
        {
            Type = "https://example.com/problems/test",
            Title = "Test Problem",
            Status = 400,
            Detail = "This is a test problem",
            Instance = "/api/test"
        };
        problem.SetExtension("customField", "customValue");

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        json.ShouldContain("\"type\":");
        json.ShouldContain("\"title\":");
        json.ShouldContain("\"status\":");
        json.ShouldContain("\"detail\":");
        json.ShouldContain("\"instance\":");
        json.ShouldContain("\"customField\":");
        json.ShouldNotContain("\"extensions\":", Case.Insensitive);
    }

    [Fact]
    public void Serialize_WithValidationErrors_ShouldSerializeErrorsAsTopLevel()
    {
        var problem = new ProblemDetails
        {
            Type = "urn:problem-type:validation-error",
            Title = "Validation Failed",
            Status = 400
        };
        problem.SetExtension("errors", new Dictionary<string, string[]>
        {
            { "email", new[] { "Email is required", "Email format is invalid" } },
            { "password", new[] { "Password is required" } }
        });

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        json.ShouldContain("\"errors\":");
        json.ShouldContain("\"email\":");
        json.ShouldContain("\"password\":");
        json.ShouldContain("Email is required");
    }

    [Fact]
    public void Serialize_WithCamelCaseNaming_ShouldUseCamelCase()
    {
        var problem = new ProblemDetails
        {
            Type = "test",
            Title = "Test"
        };
        problem.SetExtension("MyCustomField", "value");

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        json.ShouldContain("\"type\":");
        json.ShouldContain("\"title\":");
        json.ShouldContain("\"myCustomField\":");
        json.ShouldNotContain("\"Type\":", Case.Sensitive);
        json.ShouldNotContain("\"Title\":", Case.Sensitive);
        json.ShouldNotContain("\"MyCustomField\":", Case.Sensitive);
    }

    [Fact]
    public void Serialize_WithNullValues_ShouldOmitNullProperties()
    {
        var problem = new ProblemDetails
        {
            Type = "test",
            Title = null,
            Status = 400,
            Detail = null,
            Instance = null
        };

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        json.ShouldContain("\"type\":");
        json.ShouldContain("\"status\":");
        json.ShouldNotContain("\"title\":");
        json.ShouldNotContain("\"detail\":");
        json.ShouldNotContain("\"instance\":");
    }

    [Fact]
    public void Deserialize_ShouldReconstructProblemDetails()
    {
        var json = @"{
            ""type"": ""https://example.com/problems/not-found"",
            ""title"": ""Not Found"",
            ""status"": 404,
            ""detail"": ""Resource was not found"",
            ""instance"": ""/api/users/123""
        }";

        var problem = JsonSerializer.Deserialize<ProblemDetails>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        problem.ShouldNotBeNull();
        problem.Type.ShouldBe("https://example.com/problems/not-found");
        problem.Title.ShouldBe("Not Found");
        problem.Status.ShouldBe(404);
        problem.Detail.ShouldBe("Resource was not found");
        problem.Instance.ShouldBe("/api/users/123");
    }

    [Fact]
    public void Deserialize_WithExtensions_ShouldPopulateExtensions()
    {
        var json = @"{
            ""type"": ""urn:problem-type:validation-error"",
            ""title"": ""Validation Failed"",
            ""status"": 400,
            ""errors"": {
                ""email"": [""Required""],
                ""name"": [""Required""]
            },
            ""traceId"": ""abc-123""
        }";

        var problem = JsonSerializer.Deserialize<ProblemDetails>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        problem.ShouldNotBeNull();
        problem.Type.ShouldBe("urn:problem-type:validation-error");
        problem.TryGetExtension<object>("errors", out var errors).ShouldBeTrue();
        errors.ShouldNotBeNull();
        problem.TryGetExtension<object>("traceId", out var traceId).ShouldBeTrue();
    }

    [Fact]
    public void RoundTrip_ShouldPreserveAllData()
    {
        var original = new ProblemDetails
        {
            Type = "https://example.com/problems/test",
            Title = "Test Problem",
            Status = 422,
            Detail = "Detailed message",
            Instance = "/api/resource/123"
        };
        original.SetExtension("customField1", "value1");
        original.SetExtension("customField2", 42);
        original.SetExtension("customField3", new[] { "a", "b", "c" });

        var json = JsonSerializer.Serialize(original, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var deserialized = JsonSerializer.Deserialize<ProblemDetails>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        deserialized.ShouldNotBeNull();
        deserialized.Type.ShouldBe(original.Type);
        deserialized.Title.ShouldBe(original.Title);
        deserialized.Status.ShouldBe(original.Status);
        deserialized.Detail.ShouldBe(original.Detail);
        deserialized.Instance.ShouldBe(original.Instance);
        deserialized.TryGetExtension<object>("customField1", out _).ShouldBeTrue();
        deserialized.TryGetExtension<object>("customField2", out _).ShouldBeTrue();
        deserialized.TryGetExtension<object>("customField3", out _).ShouldBeTrue();
    }

    [Fact]
    public void Serialize_WithComplexExtensionTypes_ShouldWork()
    {
        var problem = new ProblemDetails
        {
            Type = "test",
            Title = "Test"
        };
        problem.SetExtension("metadata", new
        {
            createdAt = DateTime.UtcNow,
            userId = 123,
            tags = new[] { "important", "urgent" }
        });

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        json.ShouldContain("\"metadata\":");
        json.ShouldContain("\"createdAt\":");
        json.ShouldContain("\"userId\":");
        json.ShouldContain("\"tags\":");
    }

    [Fact]
    public void Deserialize_WithMinimalData_ShouldWork()
    {
        var json = @"{
            ""type"": ""about:blank"",
            ""status"": 500
        }";

        var problem = JsonSerializer.Deserialize<ProblemDetails>(json);

        problem.ShouldNotBeNull();
        problem.Type.ShouldBe("about:blank");
        problem.Status.ShouldBe(500);
        problem.Title.ShouldBeNull();
        problem.Detail.ShouldBeNull();
        problem.Instance.ShouldBeNull();
    }
}
