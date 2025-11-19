using System.Text.Json;
using Routya.ResultKit.Builders;
using Routya.ResultKit.ProblemTypes;

namespace Routya.ResultKit.Test.CoreTests;

public class ProblemDetailsTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaults()
    {
        var problem = new ProblemDetails();

        Assert.Equal("about:blank", problem.Type);
        Assert.Null(problem.Title);
        Assert.Null(problem.Status);
        Assert.Null(problem.Detail);
        Assert.Null(problem.Instance);
    }

    [Fact]
    public void InitProperties_ShouldSetValues()
    {
        var problem = new ProblemDetails
        {
            Type = "https://example.com/problems/not-found",
            Title = "Not Found",
            Status = 404,
            Detail = "The requested resource was not found",
            Instance = "/api/users/123"
        };

        Assert.Equal("https://example.com/problems/not-found", problem.Type);
        Assert.Equal("Not Found", problem.Title);
        Assert.Equal(404, problem.Status);
        Assert.Equal("The requested resource was not found", problem.Detail);
        Assert.Equal("/api/users/123", problem.Instance);
    }

    [Fact]
    public void SetExtension_ShouldStoreValue()
    {
        var problem = new ProblemDetails();
        
        problem.SetExtension("userId", 123);
        problem.SetExtension("metadata", new { created = DateTime.UtcNow });

        Assert.True(problem.TryGetExtension<int>("userId", out var userId));
        Assert.Equal(123, userId);
    }

    [Fact]
    public void SetExtension_WithNullKey_ShouldThrowArgumentException()
    {
        var problem = new ProblemDetails();

        var ex = Assert.Throws<ArgumentException>(() => problem.SetExtension<string>(null!, "value"));
        Assert.Contains("key", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SetExtension_WithEmptyKey_ShouldThrowArgumentException()
    {
        var problem = new ProblemDetails();

        var ex = Assert.Throws<ArgumentException>(() => problem.SetExtension("", "value"));
        Assert.Contains("key", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetExtension_WithExistingKey_ShouldReturnTrue()
    {
        var problem = new ProblemDetails();
        problem.SetExtension("count", 42);

        var result = problem.TryGetExtension<int>("count", out var value);

        Assert.True(result);
        Assert.Equal(42, value);
    }

    [Fact]
    public void TryGetExtension_WithNonExistingKey_ShouldReturnFalse()
    {
        var problem = new ProblemDetails();

        var result = problem.TryGetExtension<int>("missing", out var value);

        Assert.False(result);
        Assert.Equal(0, value);
    }

    [Fact]
    public void TryGetExtension_WithWrongType_ShouldReturnFalse()
    {
        var problem = new ProblemDetails();
        problem.SetExtension("value", "string");

        var result = problem.TryGetExtension<int>("value", out var value);

        Assert.False(result);
        Assert.Equal(0, value);
    }

    [Fact]
    public void TryGetExtension_WithComplexType_ShouldWork()
    {
        var problem = new ProblemDetails();
        var errors = new Dictionary<string, string[]>
        {
            { "email", new[] { "Required" } }
        };
        problem.SetExtension("errors", errors);

        var result = problem.TryGetExtension<Dictionary<string, string[]>>("errors", out var retrievedErrors);

        Assert.True(result);
        Assert.NotNull(retrievedErrors);
        Assert.Contains("email", retrievedErrors.Keys);
    }

    [Fact]
    public void JsonSerialization_ShouldProduceRfc7807Format()
    {
        var problem = new ProblemDetails
        {
            Type = StandardProblemTypes.ValidationError,
            Title = "Validation Failed",
            Status = 400,
            Detail = "One or more validation errors occurred.",
            Instance = "/api/users"
        };
        problem.SetExtension("errors", new Dictionary<string, string[]>
        {
            { "email", new[] { "Email is required" } }
        });

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.Contains("\"type\":", json);
        Assert.Contains("\"title\":", json);
        Assert.Contains("\"status\":", json);
        Assert.Contains("\"detail\":", json);
        Assert.Contains("\"instance\":", json);
        Assert.Contains("\"errors\":", json);
        Assert.DoesNotContain("\"extensions\":", json.ToLowerInvariant()); // Should not have nested Extensions
    }

    [Fact]
    public void JsonDeserialization_ShouldReconstructProblemDetails()
    {
        var json = @"{
            ""type"": ""urn:problem-type:validation-error"",
            ""title"": ""Validation Failed"",
            ""status"": 400,
            ""detail"": ""Errors occurred"",
            ""instance"": ""/api/test"",
            ""errors"": {
                ""field1"": [""error1""]
            }
        }";

        var problem = JsonSerializer.Deserialize<ProblemDetails>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(problem);
        Assert.Equal("urn:problem-type:validation-error", problem.Type);
        Assert.Equal("Validation Failed", problem.Title);
        Assert.Equal(400, problem.Status);
        Assert.Equal("Errors occurred", problem.Detail);
        Assert.Equal("/api/test", problem.Instance);
        Assert.True(problem.TryGetExtension<object>("errors", out _));
    }

    [Fact]
    public void JsonSerialization_WithNamingPolicy_ShouldUseCamelCase()
    {
        var problem = new ProblemDetails
        {
            Type = "test",
            Title = "Test Title"
        };

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.Contains("\"type\":", json);
        Assert.Contains("\"title\":", json);
        Assert.DoesNotContain("\"Type\":", json);
        Assert.DoesNotContain("\"Title\":", json);
    }

    [Fact]
    public void MultipleExtensions_ShouldAllSerialize()
    {
        var problem = new ProblemDetails
        {
            Type = "test",
            Title = "Test"
        };
        problem.SetExtension("extension1", "value1");
        problem.SetExtension("extension2", 42);
        problem.SetExtension("extension3", new[] { 1, 2, 3 });

        var json = JsonSerializer.Serialize(problem);

        Assert.Contains("extension1", json);
        Assert.Contains("extension2", json);
        Assert.Contains("extension3", json);
    }
}
