using Routya.ResultKit.AspNetCore.Exceptions;
using Routya.ResultKit.ProblemTypes;
using Shouldly;
using Xunit;

namespace Routya.ResultKit.AspNetCore.Test.Exceptions;

public class ProblemDetailsExceptionTests
{
    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        // Arrange & Act
        var exception = new ProblemDetailsException(
            StandardProblemTypes.Custom("test"),
            "Test Title",
            400,
            "Test detail message",
            "/api/test");

        // Assert
        exception.Type.ShouldBe(StandardProblemTypes.Custom("test"));
        exception.Title.ShouldBe("Test Title");
        exception.Status.ShouldBe(400);
        exception.Detail.ShouldBe("Test detail message");
        exception.Instance.ShouldBe("/api/test");
        exception.Message.ShouldBe("Test detail message");
    }

    [Fact]
    public void Constructor_WithInnerException_ShouldSetInnerException()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new ProblemDetailsException(
            StandardProblemTypes.InternalServerError,
            "Server Error",
            500,
            "An error occurred",
            innerException);

        // Assert
        exception.InnerException.ShouldBe(innerException);
        exception.Detail.ShouldBe("An error occurred");
    }

    [Fact]
    public void Extensions_ShouldBeAccessible()
    {
        // Arrange
        var exception = new ProblemDetailsException(
            StandardProblemTypes.Custom("test"),
            "Test",
            400,
            "Detail");

        // Act
        exception.Extensions["customField"] = "customValue";
        exception.Extensions["userId"] = 123;

        // Assert
        exception.Extensions.ShouldContainKey("customField");
        exception.Extensions.ShouldContainKey("userId");
        exception.Extensions["customField"].ShouldBe("customValue");
        exception.Extensions["userId"].ShouldBe(123);
    }

    [Fact]
    public void ToProblemDetails_ShouldConvertToProblemDetails()
    {
        // Arrange
        var exception = new ProblemDetailsException(
            StandardProblemTypes.NotFound,
            "Not Found",
            404,
            "Resource not found",
            "/api/users/123");
        exception.Extensions["userId"] = 123;
        exception.Extensions["searchTerm"] = "john";

        // Act
        var problemDetails = exception.ToProblemDetails();

        // Assert
        problemDetails.Type.ShouldBe(StandardProblemTypes.NotFound);
        problemDetails.Title.ShouldBe("Not Found");
        problemDetails.Status.ShouldBe(404);
        problemDetails.Detail.ShouldBe("Resource not found");
        problemDetails.Instance.ShouldBe("/api/users/123");
        problemDetails.TryGetExtension<int>("userId", out var userId).ShouldBeTrue();
        userId.ShouldBe(123);
        problemDetails.TryGetExtension<string>("searchTerm", out var searchTerm).ShouldBeTrue();
        searchTerm.ShouldBe("john");
    }

    [Fact]
    public void ToProblemDetails_WithoutInstance_ShouldUseEmptyString()
    {
        // Arrange
        var exception = new ProblemDetailsException(
            StandardProblemTypes.BadRequest,
            "Bad Request",
            400,
            "Invalid data");

        // Act
        var problemDetails = exception.ToProblemDetails();

        // Assert
        problemDetails.Instance.ShouldBe(string.Empty);
    }
}
