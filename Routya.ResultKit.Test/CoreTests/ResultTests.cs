using Routya.ResultKit.Builders;
using Routya.ResultKit.ProblemTypes;

namespace Routya.ResultKit.Test.CoreTests;

public class ResultTests
{
    [Fact]
    public void Ok_ShouldCreateSuccessfulResult()
    {
        var data = "test data";
        var result = Result<string>.Ok(data);

        Assert.True(result.Success);
        Assert.Equal(data, result.Data);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Fail_WithProblemDetails_ShouldCreateFailedResult()
    {
        var problem = new ProblemDetails
        {
            Type = StandardProblemTypes.NotFound,
            Title = "Not Found",
            Status = 404
        };
        var result = Result<string>.Fail(problem);

        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.NotNull(result.Error);
        Assert.Equal(problem, result.Error);
    }

    [Fact]
    public void BadRequest_ShouldCreateFailedResultWith400()
    {
        var result = Result<string>.BadRequest("Invalid request");

        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(400, result.Error.Status);
        Assert.Equal("Bad Request", result.Error.Title);
        Assert.Contains("Invalid request", result.Error.Detail);
        Assert.Equal(StandardProblemTypes.BadRequest, result.Error.Type);
    }

    [Fact]
    public void NotFound_ShouldCreateFailedResultWith404()
    {
        var result = Result<string>.NotFound("Resource not found");

        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(404, result.Error.Status);
        Assert.Equal("Not Found", result.Error.Title);
        Assert.Contains("Resource not found", result.Error.Detail);
        Assert.Equal(StandardProblemTypes.NotFound, result.Error.Type);
    }

    [Fact]
    public void NotFound_WithInstance_ShouldSetInstance()
    {
        var instance = "/api/users/123";
        var result = Result<string>.NotFound("User not found", instance);

        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(instance, result.Error.Instance);
    }

    [Fact]
    public void ValidationFailed_ShouldCreateFailedResultWith400()
    {
        var errors = new Dictionary<string, string[]>
        {
            { "email", new[] { "Email is required" } },
            { "name", new[] { "Name is required" } }
        };
        var result = Result<string>.ValidationFailed(errors);

        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(400, result.Error.Status);
        Assert.Equal("Validation Failed", result.Error.Title);
        Assert.Equal(StandardProblemTypes.ValidationError, result.Error.Type);
        
        Assert.True(result.Error.TryGetExtension<Dictionary<string, string[]>>("errors", out var retrievedErrors));
        Assert.Equal(2, retrievedErrors.Count);
        Assert.Contains("email", retrievedErrors.Keys);
    }

    [Fact]
    public void Unauthorized_ShouldCreateFailedResultWith401()
    {
        var result = Result<string>.Unauthorized("Authentication required");

        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(401, result.Error.Status);
        Assert.Equal("Unauthorized", result.Error.Title);
        Assert.Equal(StandardProblemTypes.Unauthorized, result.Error.Type);
    }

    [Fact]
    public void Forbidden_ShouldCreateFailedResultWith403()
    {
        var result = Result<string>.Forbidden("Access denied");

        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(403, result.Error.Status);
        Assert.Equal("Forbidden", result.Error.Title);
        Assert.Equal(StandardProblemTypes.Forbidden, result.Error.Type);
    }

    [Fact]
    public void Conflict_ShouldCreateFailedResultWith409()
    {
        var result = Result<string>.Conflict("Resource already exists");

        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(409, result.Error.Status);
        Assert.Equal("Conflict", result.Error.Title);
        Assert.Equal(StandardProblemTypes.Conflict, result.Error.Type);
    }

    [Fact]
    public void InternalServerError_ShouldCreateFailedResultWith500()
    {
        var result = Result<string>.InternalServerError("Server error occurred");

        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(500, result.Error.Status);
        Assert.Equal("Internal Server Error", result.Error.Title);
        Assert.Equal(StandardProblemTypes.InternalServerError, result.Error.Type);
    }

    [Fact]
    [Obsolete("Testing obsolete method")]
    public void Fail_ObsoleteMethod_ShouldStillWork()
    {
        var errors = new Dictionary<string, string[]>
        {
            { "field", new[] { "error" } }
        };
        var result = Result<string>.Fail("Test Error", 400, errors);

        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("Test Error", result.Error.Title);
        Assert.Equal(400, result.Error.Status);
        Assert.True(result.Error.TryGetExtension<Dictionary<string, string[]>>("errors", out var retrievedErrors));
        Assert.Contains("field", retrievedErrors.Keys);
    }

    [Fact]
    [Obsolete("Testing obsolete method")]
    public void Fail_ObsoleteMethod_WithNullErrors_ShouldWork()
    {
        var result = Result<string>.Fail("Test Error", 500, null);

        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("Test Error", result.Error.Title);
        Assert.Equal(500, result.Error.Status);
    }

    [Fact]
    public void FactoryMethods_WithDifferentDataTypes_ShouldWork()
    {
        var intResult = Result<int>.Ok(42);
        var stringResult = Result<string>.Ok("test");
        var objectResult = Result<object>.Ok(new { Id = 1 });

        Assert.True(intResult.Success);
        Assert.True(stringResult.Success);
        Assert.True(objectResult.Success);
        Assert.Equal(42, intResult.Data);
        Assert.Equal("test", stringResult.Data);
    }

    [Fact]
    public void MultipleProblemTypes_ShouldHaveDifferentTypeUris()
    {
        var badRequest = Result<string>.BadRequest("test");
        var notFound = Result<string>.NotFound("test");
        var unauthorized = Result<string>.Unauthorized("test");

        Assert.NotEqual(badRequest.Error!.Type, notFound.Error!.Type);
        Assert.NotEqual(notFound.Error!.Type, unauthorized.Error!.Type);
        Assert.NotEqual(badRequest.Error!.Type, unauthorized.Error!.Type);
    }

    // NoContent Tests
    [Fact]
    public void NoContent_ShouldCreateSuccessfulResultWith204()
    {
        var result = Result<string>.NoContent();

        Assert.True(result.Success);
        Assert.Null(result.Data);
        Assert.Null(result.Error);
        Assert.Equal(204, result.StatusCode);
        Assert.Null(result.RedirectLocation);
    }

    // Redirect Tests
    [Fact]
    public void Redirect_WithTemporary_ShouldCreateSuccessfulResultWith302()
    {
        var location = "https://example.com/new-location";
        var result = Result<string>.Redirect(location);

        Assert.True(result.Success);
        Assert.Null(result.Error);
        Assert.Equal(302, result.StatusCode);
        Assert.Equal(location, result.RedirectLocation);
    }

    [Fact]
    public void Redirect_WithPermanent_ShouldCreateSuccessfulResultWith301()
    {
        var location = "https://example.com/permanent-location";
        var result = Result<string>.Redirect(location, permanent: true);

        Assert.True(result.Success);
        Assert.Null(result.Error);
        Assert.Equal(301, result.StatusCode);
        Assert.Equal(location, result.RedirectLocation);
    }

    [Fact]
    public void RedirectPermanent_ShouldCreateSuccessfulResultWith301()
    {
        var location = "https://example.com/moved";
        var result = Result<string>.RedirectPermanent(location);

        Assert.True(result.Success);
        Assert.Equal(301, result.StatusCode);
        Assert.Equal(location, result.RedirectLocation);
    }

    [Fact]
    public void Redirect_WithNullLocation_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Result<string>.Redirect(null!));
    }

    [Fact]
    public void Redirect_WithEmptyLocation_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Result<string>.Redirect(string.Empty));
    }

    [Fact]
    public void Redirect_WithWhitespaceLocation_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Result<string>.Redirect("   "));
    }

    // StatusCode Tests
    [Fact]
    public void Created_ShouldHaveStatusCode201()
    {
        var result = Result<string>.Created("test");
        Assert.Equal(201, result.StatusCode);
        Assert.Null(result.RedirectLocation);
    }

    [Fact]
    public void Accepted_ShouldHaveStatusCode202()
    {
        var result = Result<string>.Accepted("test");
        Assert.Equal(202, result.StatusCode);
        Assert.Null(result.RedirectLocation);
    }

    [Fact]
    public void Ok_ShouldHaveStatusCode200()
    {
        var result = Result<string>.Ok("test");
        Assert.Equal(200, result.StatusCode);
        Assert.Null(result.RedirectLocation);
    }
}
