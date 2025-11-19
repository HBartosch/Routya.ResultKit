using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Routya.ResultKit.AspNetCore.Converters;

namespace Routya.ResultKit.AspNetCore.Extensions
{
    /// <summary>
    /// Extension methods for converting Result{T} to ASP.NET Core IResult and IActionResult.
    /// </summary>
    public static class ResultExtensions
    {
        /// <summary>
        /// Converts a Result{T} to an IResult for use in Minimal APIs.
        /// Returns Ok result on success, or Problem result on failure with application/problem+json content type.
        /// </summary>
        /// <typeparam name="T">The type of data in the result.</typeparam>
        /// <param name="result">The result to convert.</param>
        /// <param name="context">Optional HttpContext for setting the instance property from the request path.</param>
        /// <returns>An IResult representing the operation outcome.</returns>
        public static IResult ToHttpResult<T>(this Result<T> result, HttpContext? context = null)
        {
            if (result.Success)
            {
                return result.StatusCode switch
                {
                    201 => Results.Created(context?.Request.Path.Value ?? string.Empty, result.Data),
                    202 => Results.Accepted(context?.Request.Path.Value ?? string.Empty, result.Data),
                    204 => Results.NoContent(),
                    _ => Results.Ok(result.Data)
                };
            }

            var problemDetails = ProblemDetailsConverter.ToMicrosoft(result.Error!);
            
            // Set instance from HttpContext if not already set
            if (context != null && string.IsNullOrEmpty(problemDetails.Instance))
            {
                problemDetails.Instance = context.Request.Path.Value;
            }

            return Results.Problem(
                detail: problemDetails.Detail,
                instance: problemDetails.Instance,
                statusCode: problemDetails.Status,
                title: problemDetails.Title,
                type: problemDetails.Type,
                extensions: problemDetails.Extensions
            );
        }

        /// <summary>
        /// Converts a Result{T} to an IActionResult for use in MVC controllers.
        /// Returns OkObjectResult on success, or ObjectResult with ProblemDetails on failure.
        /// </summary>
        /// <typeparam name="T">The type of data in the result.</typeparam>
        /// <param name="result">The result to convert.</param>
        /// <param name="context">Optional HttpContext for setting the instance property from the request path.</param>
        /// <returns>An IActionResult representing the operation outcome.</returns>
        public static IActionResult ToActionResult<T>(this Result<T> result, HttpContext? context = null)
        {
            if (result.Success)
            {
                return result.StatusCode switch
                {
                    201 => new CreatedResult(context?.Request.Path.Value ?? string.Empty, result.Data),
                    202 => new AcceptedResult(context?.Request.Path.Value ?? string.Empty, result.Data),
                    204 => new NoContentResult(),
                    _ => new OkObjectResult(result.Data)
                };
            }

            var problemDetails = ProblemDetailsConverter.ToMicrosoft(result.Error!);
            
            // Set instance from HttpContext if not already set
            if (context != null && string.IsNullOrEmpty(problemDetails.Instance))
            {
                problemDetails.Instance = context.Request.Path.Value;
            }

            return new ObjectResult(problemDetails)
            {
                StatusCode = problemDetails.Status,
                ContentTypes = { "application/problem+json" }
            };
        }

        /// <summary>
        /// Converts a ProblemDetails to an IResult for use in Minimal APIs.
        /// </summary>
        /// <param name="problemDetails">The problem details to convert.</param>
        /// <returns>An IResult representing the problem.</returns>
        public static IResult ToProblemResult(this Routya.ResultKit.ProblemDetails problemDetails)
        {
            var microsoft = ProblemDetailsConverter.ToMicrosoft(problemDetails);
            
            return Results.Problem(
                detail: microsoft.Detail,
                instance: microsoft.Instance,
                statusCode: microsoft.Status,
                title: microsoft.Title,
                type: microsoft.Type,
                extensions: microsoft.Extensions
            );
        }

        /// <summary>
        /// Converts a ProblemDetails to an IActionResult for use in MVC controllers.
        /// </summary>
        /// <param name="problemDetails">The problem details to convert.</param>
        /// <returns>An IActionResult representing the problem.</returns>
        public static IActionResult ToProblemActionResult(this Routya.ResultKit.ProblemDetails problemDetails)
        {
            var microsoft = ProblemDetailsConverter.ToMicrosoft(problemDetails);
            
            return new ObjectResult(microsoft)
            {
                StatusCode = microsoft.Status,
                ContentTypes = { "application/problem+json" }
            };
        }
    }
}
