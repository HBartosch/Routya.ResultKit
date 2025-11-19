using Routya.ResultKit.AspNetCore.Converters;
using Routya.ResultKit.Builders;
using Routya.ResultKit.ProblemTypes;
using Shouldly;
using Xunit;

namespace Routya.ResultKit.AspNetCore.Test.Converters;

public class ProblemDetailsConverterTests
{
    [Fact]
    public void ToMicrosoft_ShouldConvertAllProperties()
    {
        // Arrange
        var routyaProblem = new ProblemDetails
        {
            Type = StandardProblemTypes.NotFound,
            Title = "Not Found",
            Status = 404,
            Detail = "The requested resource was not found",
            Instance = "/api/users/123"
        };
        routyaProblem.SetExtension("traceId", "abc-123");
        routyaProblem.SetExtension("userId", 123);

        // Act
        var microsoftProblem = ProblemDetailsConverter.ToMicrosoft(routyaProblem);

        // Assert
        microsoftProblem.Type.ShouldBe(routyaProblem.Type);
        microsoftProblem.Title.ShouldBe(routyaProblem.Title);
        microsoftProblem.Status.ShouldBe(routyaProblem.Status);
        microsoftProblem.Detail.ShouldBe(routyaProblem.Detail);
        microsoftProblem.Instance.ShouldBe(routyaProblem.Instance);
        microsoftProblem.Extensions.ShouldContainKey("traceId");
        microsoftProblem.Extensions.ShouldContainKey("userId");
        microsoftProblem.Extensions["traceId"].ShouldBe("abc-123");
        microsoftProblem.Extensions["userId"].ShouldBe(123);
    }

    [Fact]
    public void ToMicrosoft_WithNullSource_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => 
            ProblemDetailsConverter.ToMicrosoft(null!));
    }

    [Fact]
    public void ToMicrosoft_WithValidationErrors_ShouldPreserveErrors()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "email", new[] { "Email is required" } },
            { "name", new[] { "Name is required" } }
        };
        var routyaProblem = ProblemDetailsBuilder.ValidationError(errors).Build();

        // Act
        var microsoftProblem = ProblemDetailsConverter.ToMicrosoft(routyaProblem);

        // Assert
        microsoftProblem.Extensions.ShouldContainKey("errors");
        var retrievedErrors = microsoftProblem.Extensions["errors"] as IDictionary<string, string[]>;
        retrievedErrors.ShouldNotBeNull();
        retrievedErrors.ShouldContainKey("email");
        retrievedErrors.ShouldContainKey("name");
    }

    [Fact]
    public void FromMicrosoft_ShouldConvertAllProperties()
    {
        // Arrange
        var microsoftProblem = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Type = StandardProblemTypes.BadRequest,
            Title = "Bad Request",
            Status = 400,
            Detail = "The request is invalid",
            Instance = "/api/orders"
        };
        microsoftProblem.Extensions["requestId"] = "req-456";
        microsoftProblem.Extensions["timestamp"] = DateTime.UtcNow;

        // Act
        var routyaProblem = ProblemDetailsConverter.FromMicrosoft(microsoftProblem);

        // Assert
        routyaProblem.Type.ShouldBe(microsoftProblem.Type);
        routyaProblem.Title.ShouldBe(microsoftProblem.Title);
        routyaProblem.Status.ShouldBe(microsoftProblem.Status);
        routyaProblem.Detail.ShouldBe(microsoftProblem.Detail);
        routyaProblem.Instance.ShouldBe(microsoftProblem.Instance);
        routyaProblem.TryGetExtension<string>("requestId", out var requestId).ShouldBeTrue();
        requestId.ShouldBe("req-456");
        routyaProblem.TryGetExtension<DateTime>("timestamp", out _).ShouldBeTrue();
    }

    [Fact]
    public void FromMicrosoft_WithNullSource_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => 
            ProblemDetailsConverter.FromMicrosoft(null!));
    }

    [Fact]
    public void FromMicrosoft_WithNullType_ShouldDefaultToAboutBlank()
    {
        // Arrange
        var microsoftProblem = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Type = null,
            Status = 500
        };

        // Act
        var routyaProblem = ProblemDetailsConverter.FromMicrosoft(microsoftProblem);

        // Assert
        routyaProblem.Type.ShouldBe("about:blank");
    }

    [Fact]
    public void RoundTrip_ShouldPreserveAllData()
    {
        // Arrange
        var original = new ProblemDetails
        {
            Type = StandardProblemTypes.Conflict,
            Title = "Conflict",
            Status = 409,
            Detail = "Resource already exists",
            Instance = "/api/users"
        };
        original.SetExtension("field1", "value1");
        original.SetExtension("field2", 42);

        // Act
        var microsoft = ProblemDetailsConverter.ToMicrosoft(original);
        var roundTripped = ProblemDetailsConverter.FromMicrosoft(microsoft);

        // Assert
        roundTripped.Type.ShouldBe(original.Type);
        roundTripped.Title.ShouldBe(original.Title);
        roundTripped.Status.ShouldBe(original.Status);
        roundTripped.Detail.ShouldBe(original.Detail);
        roundTripped.Instance.ShouldBe(original.Instance);
        roundTripped.TryGetExtension<string>("field1", out var field1).ShouldBeTrue();
        field1.ShouldBe("value1");
        roundTripped.TryGetExtension<object>("field2", out _).ShouldBeTrue();
    }
}
