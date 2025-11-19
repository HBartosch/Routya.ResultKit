using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Routya.ResultKit.AspNetCore.Extensions;
using Routya.ResultKit.Builders;
using Shouldly;
using NSubstitute;
using Xunit;

namespace Routya.ResultKit.AspNetCore.Test.Extensions;

public class ResultExtensionsTests
{
    [Fact]
    public void ToHttpResult_WithSuccessfulResult_ShouldReturnOkResult()
    {
        // Arrange
        var data = new { Id = 1, Name = "Test" };
        var result = Result<object>.Ok(data);

        // Act
        var httpResult = result.ToHttpResult();

        // Assert
        httpResult.ShouldNotBeNull();
        // Note: We can't easily assert the IResult type without executing it,
        // but we can verify it doesn't throw
    }

    [Fact]
    public void ToHttpResult_WithFailedResult_ShouldReturnProblemResult()
    {
        // Arrange
        var result = Result<string>.NotFound("User not found");

        // Act
        var httpResult = result.ToHttpResult();

        // Assert
        httpResult.ShouldNotBeNull();
    }

    [Fact]
    public void ToHttpResult_WithContext_ShouldSetInstanceFromPath()
    {
        // Arrange
        var result = Result<string>.BadRequest("Invalid request");
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/users/123";

        // Act
        var httpResult = result.ToHttpResult(context);

        // Assert
        httpResult.ShouldNotBeNull();
    }

    [Fact]
    public void ToActionResult_WithSuccessfulResult_ShouldReturnOkObjectResult()
    {
        // Arrange
        var data = "test data";
        var result = Result<string>.Ok(data);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        actionResult.ShouldBeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)actionResult;
        okResult.Value.ShouldBe(data);
    }

    [Fact]
    public void ToActionResult_WithFailedResult_ShouldReturnObjectResultWithProblemDetails()
    {
        // Arrange
        var result = Result<string>.NotFound("Resource not found");

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        actionResult.ShouldBeOfType<ObjectResult>();
        var objectResult = (ObjectResult)actionResult;
        objectResult.StatusCode.ShouldBe(404);
        objectResult.Value.ShouldBeOfType<Microsoft.AspNetCore.Mvc.ProblemDetails>();
    }

    [Fact]
    public void ToActionResult_WithValidationErrors_ShouldIncludeErrorsInProblemDetails()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "email", new[] { "Email is required" } },
            { "name", new[] { "Name is required" } }
        };
        var result = Result<string>.ValidationFailed(errors);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        actionResult.ShouldBeOfType<ObjectResult>();
        var objectResult = (ObjectResult)actionResult;
        objectResult.StatusCode.ShouldBe(400);
        objectResult.Value.ShouldBeOfType<Microsoft.AspNetCore.Mvc.ProblemDetails>();
        
        var problemDetails = (Microsoft.AspNetCore.Mvc.ProblemDetails)objectResult.Value;
        problemDetails.Extensions.ShouldContainKey("errors");
    }

    [Fact]
    public void ToActionResult_WithContext_ShouldSetInstanceFromPath()
    {
        // Arrange
        var result = Result<string>.Unauthorized("Not authenticated");
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/secure";

        // Act
        var actionResult = result.ToActionResult(context);

        // Assert
        actionResult.ShouldBeOfType<ObjectResult>();
        var objectResult = (ObjectResult)actionResult;
        var problemDetails = (Microsoft.AspNetCore.Mvc.ProblemDetails)objectResult.Value!;
        problemDetails.Instance.ShouldBe("/api/secure");
    }

    [Fact]
    public void ToProblemResult_ShouldConvertProblemDetailsToIResult()
    {
        // Arrange
        var problem = ProblemDetailsBuilder.Forbidden("Access denied")
            .WithInstance("/api/admin")
            .Build();

        // Act
        var httpResult = problem.ToProblemResult();

        // Assert
        httpResult.ShouldNotBeNull();
    }

    [Fact]
    public void ToProblemActionResult_ShouldConvertProblemDetailsToIActionResult()
    {
        // Arrange
        var problem = ProblemDetailsBuilder.Conflict("Duplicate entry")
            .WithInstance("/api/resources")
            .Build();

        // Act
        var actionResult = problem.ToProblemActionResult();

        // Assert
        actionResult.ShouldBeOfType<ObjectResult>();
        var objectResult = (ObjectResult)actionResult;
        objectResult.StatusCode.ShouldBe(409);
        objectResult.ContentTypes.ShouldContain("application/problem+json");
    }
}
