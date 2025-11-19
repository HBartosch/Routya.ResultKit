using Routya.ResultKit.Builders;
using Routya.ResultKit.ProblemTypes;

namespace Routya.ResultKit.Test.BuildersTests;

public class ProblemDetailsBuilderTests
{
    [Fact]
    public void Build_WithMinimalConfiguration_ShouldCreateValidProblemDetails()
    {
        var problem = new ProblemDetailsBuilder()
            .WithTitle("Test")
            .Build();

        Assert.Equal("about:blank", problem.Type);
        Assert.Equal("Test", problem.Title);
        Assert.Null(problem.Status);
        Assert.Null(problem.Detail);
        Assert.Null(problem.Instance);
    }

    [Fact]
    public void WithType_WithValidUri_ShouldSetType()
    {
        var problem = new ProblemDetailsBuilder()
            .WithType("https://example.com/problems/test")
            .Build();

        Assert.Equal("https://example.com/problems/test", problem.Type);
    }

    [Fact]
    public void WithType_WithValidUrn_ShouldSetType()
    {
        var problem = new ProblemDetailsBuilder()
            .WithType("urn:problem-type:test")
            .Build();

        Assert.Equal("urn:problem-type:test", problem.Type);
    }

    [Fact]
    public void WithType_WithInvalidUri_ShouldThrowArgumentException()
    {
        var builder = new ProblemDetailsBuilder();

        var ex = Assert.Throws<ArgumentException>(() => builder.WithType("not-a-uri"));
        Assert.Contains("valid absolute URI or URN", ex.Message);
    }

    [Fact]
    public void WithType_WithNullOrEmpty_ShouldThrowArgumentException()
    {
        var builder = new ProblemDetailsBuilder();

        Assert.Throws<ArgumentException>(() => builder.WithType(null!));
        Assert.Throws<ArgumentException>(() => builder.WithType(""));
        Assert.Throws<ArgumentException>(() => builder.WithType("   "));
    }

    [Fact]
    public void WithTitle_ShouldSetTitle()
    {
        var problem = new ProblemDetailsBuilder()
            .WithTitle("Test Title")
            .Build();

        Assert.Equal("Test Title", problem.Title);
    }

    [Fact]
    public void WithStatus_WithValidCode_ShouldSetStatus()
    {
        var problem = new ProblemDetailsBuilder()
            .WithStatus(400)
            .Build();

        Assert.Equal(400, problem.Status);
    }

    [Fact]
    public void WithStatus_WithInvalidCode_ShouldThrowArgumentOutOfRangeException()
    {
        var builder = new ProblemDetailsBuilder();

        Assert.Throws<ArgumentOutOfRangeException>(() => builder.WithStatus(99));
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.WithStatus(600));
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.WithStatus(-1));
    }

    [Fact]
    public void WithStatus_WithBoundaryValues_ShouldWork()
    {
        var problem100 = new ProblemDetailsBuilder().WithStatus(100).Build();
        var problem599 = new ProblemDetailsBuilder().WithStatus(599).Build();

        Assert.Equal(100, problem100.Status);
        Assert.Equal(599, problem599.Status);
    }

    [Fact]
    public void WithDetail_ShouldSetDetail()
    {
        var problem = new ProblemDetailsBuilder()
            .WithDetail("Detailed error message")
            .Build();

        Assert.Equal("Detailed error message", problem.Detail);
    }

    [Fact]
    public void WithInstance_WithValidUri_ShouldSetInstance()
    {
        var problem = new ProblemDetailsBuilder()
            .WithInstance("/api/users/123")
            .Build();

        Assert.Equal("/api/users/123", problem.Instance);
    }

    [Fact]
    public void WithInstance_WithInvalidUri_ShouldThrowArgumentException()
    {
        var builder = new ProblemDetailsBuilder();

        var ex = Assert.Throws<ArgumentException>(() => builder.WithInstance("not a valid uri with spaces"));
        Assert.Contains("valid URI", ex.Message);
    }

    [Fact]
    public void WithExtension_ShouldAddExtension()
    {
        var problem = new ProblemDetailsBuilder()
            .WithExtension("key1", "value1")
            .WithExtension("key2", 42)
            .Build();

        Assert.True(problem.TryGetExtension<string>("key1", out var value1));
        Assert.Equal("value1", value1);
        Assert.True(problem.TryGetExtension<int>("key2", out var value2));
        Assert.Equal(42, value2);
    }

    [Fact]
    public void WithExtension_WithNullKey_ShouldThrowArgumentException()
    {
        var builder = new ProblemDetailsBuilder();

        Assert.Throws<ArgumentException>(() => builder.WithExtension<string>(null!, "value"));
    }

    [Fact]
    public void WithExtension_WithReservedKey_ShouldThrowArgumentException()
    {
        var builder = new ProblemDetailsBuilder();

        Assert.Throws<ArgumentException>(() => builder.WithExtension("type", "value"));
        Assert.Throws<ArgumentException>(() => builder.WithExtension("Type", "value"));
        Assert.Throws<ArgumentException>(() => builder.WithExtension("title", "value"));
        Assert.Throws<ArgumentException>(() => builder.WithExtension("status", 400));
        Assert.Throws<ArgumentException>(() => builder.WithExtension("detail", "value"));
        Assert.Throws<ArgumentException>(() => builder.WithExtension("instance", "value"));
    }

    [Fact]
    public void FluentChaining_ShouldWork()
    {
        var problem = new ProblemDetailsBuilder()
            .WithType("https://example.com/problems/test")
            .WithTitle("Test Problem")
            .WithStatus(400)
            .WithDetail("This is a test")
            .WithInstance("/test")
            .WithExtension("custom", "data")
            .Build();

        Assert.Equal("https://example.com/problems/test", problem.Type);
        Assert.Equal("Test Problem", problem.Title);
        Assert.Equal(400, problem.Status);
        Assert.Equal("This is a test", problem.Detail);
        Assert.Equal("/test", problem.Instance);
        Assert.True(problem.TryGetExtension<string>("custom", out var custom));
        Assert.Equal("data", custom);
    }

    [Fact]
    public void BadRequest_ShouldCreateBadRequestProblem()
    {
        var problem = ProblemDetailsBuilder.BadRequest().Build();

        Assert.Equal(StandardProblemTypes.BadRequest, problem.Type);
        Assert.Equal("Bad Request", problem.Title);
        Assert.Equal(400, problem.Status);
        Assert.NotNull(problem.Detail);
    }

    [Fact]
    public void BadRequest_WithCustomDetail_ShouldUseCustomDetail()
    {
        var problem = ProblemDetailsBuilder.BadRequest("Custom error message").Build();

        Assert.Contains("Custom error message", problem.Detail);
    }

    [Fact]
    public void Unauthorized_ShouldCreateUnauthorizedProblem()
    {
        var problem = ProblemDetailsBuilder.Unauthorized().Build();

        Assert.Equal(StandardProblemTypes.Unauthorized, problem.Type);
        Assert.Equal("Unauthorized", problem.Title);
        Assert.Equal(401, problem.Status);
    }

    [Fact]
    public void Forbidden_ShouldCreateForbiddenProblem()
    {
        var problem = ProblemDetailsBuilder.Forbidden().Build();

        Assert.Equal(StandardProblemTypes.Forbidden, problem.Type);
        Assert.Equal("Forbidden", problem.Title);
        Assert.Equal(403, problem.Status);
    }

    [Fact]
    public void NotFound_ShouldCreateNotFoundProblem()
    {
        var problem = ProblemDetailsBuilder.NotFound().Build();

        Assert.Equal(StandardProblemTypes.NotFound, problem.Type);
        Assert.Equal("Not Found", problem.Title);
        Assert.Equal(404, problem.Status);
    }

    [Fact]
    public void Conflict_ShouldCreateConflictProblem()
    {
        var problem = ProblemDetailsBuilder.Conflict().Build();

        Assert.Equal(StandardProblemTypes.Conflict, problem.Type);
        Assert.Equal("Conflict", problem.Title);
        Assert.Equal(409, problem.Status);
    }

    [Fact]
    public void ValidationError_ShouldCreateValidationProblem()
    {
        var errors = new Dictionary<string, string[]>
        {
            { "email", new[] { "Required" } }
        };
        var problem = ProblemDetailsBuilder.ValidationError(errors).Build();

        Assert.Equal(StandardProblemTypes.ValidationError, problem.Type);
        Assert.Equal("Validation Failed", problem.Title);
        Assert.Equal(400, problem.Status);
        Assert.True(problem.TryGetExtension<Dictionary<string, string[]>>("errors", out var retrievedErrors));
        Assert.Contains("email", retrievedErrors.Keys);
    }

    [Fact]
    public void ValidationError_WithNullErrors_ShouldNotThrow()
    {
        var problem = ProblemDetailsBuilder.ValidationError(null).Build();

        Assert.Equal(StandardProblemTypes.ValidationError, problem.Type);
        Assert.Equal(400, problem.Status);
    }

    [Fact]
    public void InternalServerError_ShouldCreateServerErrorProblem()
    {
        var problem = ProblemDetailsBuilder.InternalServerError().Build();

        Assert.Equal(StandardProblemTypes.InternalServerError, problem.Type);
        Assert.Equal("Internal Server Error", problem.Title);
        Assert.Equal(500, problem.Status);
    }

    [Fact]
    public void FactoryMethods_CanBeChainedWithAdditionalProperties()
    {
        var problem = ProblemDetailsBuilder.NotFound("User not found")
            .WithInstance("/api/users/123")
            .WithExtension("userId", 123)
            .Build();

        Assert.Equal(StandardProblemTypes.NotFound, problem.Type);
        Assert.Equal("/api/users/123", problem.Instance);
        Assert.True(problem.TryGetExtension<int>("userId", out var userId));
        Assert.Equal(123, userId);
    }
}
