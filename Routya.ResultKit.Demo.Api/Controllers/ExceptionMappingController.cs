using Microsoft.AspNetCore.Mvc;
using Routya.ResultKit;
using Routya.ResultKit.AspNetCore.Extensions;
using Routya.ResultKit.AspNetCore.Exceptions;

namespace Routya.ResultKit.Demo.Api.Controllers;

/// <summary>
/// Demonstrates exception handling and mapping to ProblemDetails
/// The middleware will automatically catch exceptions and convert them to ProblemDetails.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ExceptionMappingController : ControllerBase
{
    /// <summary>
    /// Throws ArgumentException - mapped to 400 Bad Request
    /// </summary>
    /// <remarks>
    /// The default ArgumentException mapper will convert this to a 400 Bad Request.
    /// </remarks>
    [HttpGet("argument-exception")]
    [ProducesResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult ThrowArgumentException()
    {
        throw new ArgumentException("Invalid argument provided", "testParameter");
    }

    /// <summary>
    /// Throws ArgumentNullException - mapped to 400 Bad Request
    /// </summary>
    [HttpGet("argument-null-exception")]
    [ProducesResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult ThrowArgumentNullException()
    {
        throw new ArgumentNullException("requiredParameter", "The required parameter cannot be null");
    }

    /// <summary>
    /// Throws UnauthorizedAccessException - mapped to 401 Unauthorized
    /// </summary>
    [HttpGet("unauthorized-exception")]
    [ProducesResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), StatusCodes.Status401Unauthorized)]
    public IActionResult ThrowUnauthorizedAccessException()
    {
        throw new UnauthorizedAccessException("You do not have access to this resource");
    }

    /// <summary>
    /// Throws InvalidOperationException - mapped to 400 Bad Request
    /// </summary>
    [HttpGet("invalid-operation-exception")]
    [ProducesResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult ThrowInvalidOperationException()
    {
        throw new InvalidOperationException("This operation cannot be performed in the current state");
    }

    /// <summary>
    /// Throws KeyNotFoundException - mapped to 404 Not Found
    /// </summary>
    [HttpGet("key-not-found-exception")]
    [ProducesResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), StatusCodes.Status404NotFound)]
    public IActionResult ThrowKeyNotFoundException()
    {
        throw new KeyNotFoundException("The requested resource was not found");
    }

    /// <summary>
    /// Throws NotSupportedException - mapped to 400 Bad Request
    /// </summary>
    [HttpGet("not-supported-exception")]
    [ProducesResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult ThrowNotSupportedException()
    {
        throw new NotSupportedException("This operation is not supported");
    }

    /// <summary>
    /// Throws ProblemDetailsException with custom extensions
    /// </summary>
    /// <remarks>
    /// ProblemDetailsException allows you to throw exceptions with rich ProblemDetails context.
    /// </remarks>
    [HttpGet("problem-details-exception")]
    [ProducesResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult ThrowProblemDetailsException()
    {
        var exception = new ProblemDetailsException(
            type: "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            title: "Bad Request",
            status: 400,
            detail: "Custom business logic error");

        exception.Extensions["errorCode"] = "BUSINESS_RULE_VIOLATION";
        exception.Extensions["ruleName"] = "MinimumOrderAmount";
        exception.Extensions["minimumAmount"] = 50.00;
        exception.Extensions["providedAmount"] = 25.00;

        throw exception;
    }

    /// <summary>
    /// Throws ProblemDetailsException with validation errors
    /// </summary>
    [HttpGet("validation-exception")]
    [ProducesResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult ThrowValidationException()
    {
        var exception = new ProblemDetailsException(
            type: "https://tools.ietf.org/html/rfc4918#section-11.2",
            title: "Unprocessable Entity",
            status: 422,
            detail: "Validation failed");

        var errors = new Dictionary<string, string[]>
        {
            ["email"] = new[] { "Email is required", "Invalid email format" },
            ["age"] = new[] { "Age must be at least 18" }
        };

        exception.Extensions["errors"] = errors;
        exception.Extensions["validatedAt"] = DateTime.UtcNow;

        throw exception;
    }

    /// <summary>
    /// Throws generic Exception - mapped to 500 Internal Server Error
    /// </summary>
    /// <remarks>
    /// Any exception not explicitly mapped will default to 500 Internal Server Error.
    /// The error details are sanitized in production to avoid exposing sensitive information.
    /// </remarks>
    [HttpGet("generic-exception")]
    [ProducesResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), StatusCodes.Status500InternalServerError)]
    public IActionResult ThrowGenericException()
    {
        throw new Exception("An unexpected error occurred");
    }

    /// <summary>
    /// Simulate a database error
    /// </summary>
    [HttpGet("database-error")]
    [ProducesResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), StatusCodes.Status500InternalServerError)]
    public IActionResult SimulateDatabaseError()
    {
        // In a real application, this would be caught from actual database operations
        throw new InvalidOperationException("Database connection failed");
    }

    /// <summary>
    /// Returns a manually created error without throwing
    /// </summary>
    /// <remarks>
    /// You can return ProblemDetails without throwing exceptions using Result.Failure().
    /// This is the preferred approach when errors are expected business logic outcomes.
    /// </remarks>
    [HttpGet("manual-error")]
    [ProducesResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult ManualError()
    {
        var problem = Routya.ResultKit.Builders.ProblemDetailsBuilder.BadRequest("This is a manually created error")
            .WithExtension("errorType", "manual")
            .WithExtension("timestamp", DateTime.UtcNow)
            .Build();
        return Result<object>.Fail(problem).ToActionResult(HttpContext);
    }
}
