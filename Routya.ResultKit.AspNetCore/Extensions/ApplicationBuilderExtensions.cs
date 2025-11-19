using Microsoft.AspNetCore.Builder;
using Routya.ResultKit.AspNetCore.Middleware;

namespace Routya.ResultKit.AspNetCore.Extensions
{
    /// <summary>
    /// Extension methods for configuring the ResultKit middleware pipeline.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds the ResultKit exception handler middleware to the pipeline.
        /// This should be added early in the pipeline to catch all exceptions.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <returns>The application builder for method chaining.</returns>
        public static IApplicationBuilder UseResultKitExceptionHandler(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ResultKitExceptionHandlerMiddleware>();
        }

        /// <summary>
        /// Adds the exception mapping middleware to the pipeline.
        /// Alias for UseResultKitExceptionHandler for convenience.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <returns>The application builder for method chaining.</returns>
        public static IApplicationBuilder UseExceptionMapping(this IApplicationBuilder app)
        {
            return app.UseResultKitExceptionHandler();
        }
    }
}
