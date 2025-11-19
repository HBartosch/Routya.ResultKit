using Microsoft.AspNetCore.Http;
using Routya.ResultKit.AspNetCore.Exceptions;
using Routya.ResultKit.AspNetCore.Mappers;
using Routya.ResultKit.Builders;
using Routya.ResultKit.ProblemTypes;
using Shouldly;
using Xunit;

namespace Routya.ResultKit.AspNetCore.Test.Mappers;

public class ExceptionMapperRegistryTests
{
    [Fact]
    public void Map_WithProblemDetailsException_ShouldMapCorrectly()
    {
        // Arrange
        var registry = new ExceptionMapperRegistry();
        var exception = new ProblemDetailsException(
            StandardProblemTypes.Custom("test-error"),
            "Test Error",
            422,
            "This is a test error");
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test";

        // Act
        var problemDetails = registry.Map(exception, context);

        // Assert
        problemDetails.ShouldNotBeNull();
        problemDetails.Title.ShouldBe("Test Error");
        problemDetails.Status.ShouldBe(422);
        problemDetails.Detail.ShouldBe("This is a test error");
    }

    [Fact]
    public void Map_WithArgumentException_ShouldReturnBadRequest()
    {
        // Arrange
        var registry = new ExceptionMapperRegistry();
        var exception = new ArgumentException("Invalid argument");
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/endpoint";

        // Act
        var problemDetails = registry.Map(exception, context);

        // Assert
        problemDetails.ShouldNotBeNull();
        problemDetails.Status.ShouldBe(400);
        problemDetails.Type.ShouldBe(StandardProblemTypes.BadRequest);
        problemDetails.Detail.ShouldContain("Invalid argument");
    }

    [Fact]
    public void Map_WithArgumentNullException_ShouldReturnBadRequest()
    {
        // Arrange
        var registry = new ExceptionMapperRegistry();
        var exception = new ArgumentNullException("paramName", "Parameter cannot be null");
        var context = new DefaultHttpContext();

        // Act
        var problemDetails = registry.Map(exception, context);

        // Assert
        problemDetails.Status.ShouldBe(400);
        problemDetails.Type.ShouldBe(StandardProblemTypes.BadRequest);
    }

    [Fact]
    public void Map_WithUnauthorizedAccessException_ShouldReturnForbidden()
    {
        // Arrange
        var registry = new ExceptionMapperRegistry();
        var exception = new UnauthorizedAccessException("Access denied");
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/admin";

        // Act
        var problemDetails = registry.Map(exception, context);

        // Assert
        problemDetails.Status.ShouldBe(403);
        problemDetails.Type.ShouldBe(StandardProblemTypes.Forbidden);
        problemDetails.Detail.ShouldBe("Access denied");
        problemDetails.Instance.ShouldBe("/api/admin");
    }

    [Fact]
    public void Map_WithInvalidOperationException_ShouldReturnConflict()
    {
        // Arrange
        var registry = new ExceptionMapperRegistry();
        var exception = new InvalidOperationException("Operation not valid");
        var context = new DefaultHttpContext();

        // Act
        var problemDetails = registry.Map(exception, context);

        // Assert
        problemDetails.Status.ShouldBe(409);
        problemDetails.Type.ShouldBe(StandardProblemTypes.Conflict);
    }

    [Fact]
    public void Map_WithNotImplementedException_ShouldReturn501()
    {
        // Arrange
        var registry = new ExceptionMapperRegistry();
        var exception = new NotImplementedException("Feature not implemented");
        var context = new DefaultHttpContext();

        // Act
        var problemDetails = registry.Map(exception, context);

        // Assert
        problemDetails.Status.ShouldBe(501);
        problemDetails.Title.ShouldBe("Not Implemented");
    }

    [Fact]
    public void Map_WithUnknownException_ShouldReturnInternalServerError()
    {
        // Arrange
        var registry = new ExceptionMapperRegistry();
        var exception = new Exception("Unknown error");
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/crash";

        // Act
        var problemDetails = registry.Map(exception, context);

        // Assert
        problemDetails.Status.ShouldBe(500);
        problemDetails.Type.ShouldBe(StandardProblemTypes.InternalServerError);
        problemDetails.Detail.ShouldBe("Unknown error");
        problemDetails.Instance.ShouldBe("/api/crash");
    }

    [Fact]
    public void Register_WithCustomMapper_ShouldUseCustomMapper()
    {
        // Arrange
        var registry = new ExceptionMapperRegistry();
        var customMapper = new TestCustomExceptionMapper();
        registry.Register(customMapper);
        
        var exception = new TestCustomException("Custom error");
        var context = new DefaultHttpContext();

        // Act
        var problemDetails = registry.Map(exception, context);

        // Assert
        problemDetails.Title.ShouldBe("Custom Exception");
        problemDetails.Status.ShouldBe(418); // I'm a teapot
    }
}

// Test helpers
public class TestCustomException : Exception
{
    public TestCustomException(string message) : base(message) { }
}

public class TestCustomExceptionMapper : IExceptionMapper
{
    public bool CanHandle(Exception exception) => exception is TestCustomException;

    public ProblemDetails Map(Exception exception, HttpContext context)
    {
        return new ProblemDetailsBuilder()
            .WithType(StandardProblemTypes.Custom("custom-exception"))
            .WithTitle("Custom Exception")
            .WithStatus(418)
            .WithDetail(exception.Message)
            .WithInstance(context.Request.Path.Value ?? string.Empty)
            .Build();
    }
}
