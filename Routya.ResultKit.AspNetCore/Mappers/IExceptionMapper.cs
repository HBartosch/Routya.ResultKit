using Microsoft.AspNetCore.Http;

namespace Routya.ResultKit.AspNetCore.Mappers;

/// <summary>
/// Interface for mapping exceptions to ProblemDetails.
/// </summary>
public interface IExceptionMapper
{
    /// <summary>
    /// Determines whether this mapper can handle the specified exception.
    /// </summary>
    /// <param name="exception">The exception to check.</param>
    /// <returns>True if this mapper can handle the exception; otherwise false.</returns>
    bool CanHandle(Exception exception);

    /// <summary>
    /// Maps an exception to a ProblemDetails instance.
    /// </summary>
    /// <param name="exception">The exception to map.</param>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A ProblemDetails instance representing the exception.</returns>
    Routya.ResultKit.ProblemDetails Map(Exception exception, HttpContext context);
}
