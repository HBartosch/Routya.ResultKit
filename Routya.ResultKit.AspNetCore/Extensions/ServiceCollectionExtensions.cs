using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Routya.ResultKit.AspNetCore.Configuration;
using Routya.ResultKit.AspNetCore.Mappers;

namespace Routya.ResultKit.AspNetCore.Extensions
{
    /// <summary>
    /// Extension methods for registering ResultKit services with the dependency injection container.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds ResultKit ProblemDetails services to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configure">Optional configuration action.</param>
        /// <returns>The service collection for method chaining.</returns>
        public static IServiceCollection AddResultKitProblemDetails(
            this IServiceCollection services,
            Action<ResultKitOptions>? configure = null)
        {
            // Configure options
            var options = new ResultKitOptions();
            configure?.Invoke(options);
            
            // Apply base URI to StandardProblemTypes
            options.ApplyToStandardProblemTypes();
            
            services.Configure<ResultKitOptions>(opts =>
            {
                opts.ProblemTypeBaseUri = options.ProblemTypeBaseUri;
                opts.IncludeExceptionDetails = options.IncludeExceptionDetails;
                opts.IncludeTraceId = options.IncludeTraceId;
                opts.TraceIdExtensionName = options.TraceIdExtensionName;
                opts.NamingPolicy = options.NamingPolicy;
            });

            // Register exception mapper registry as singleton
            services.TryAddSingleton<ExceptionMapperRegistry>();

            // Add ProblemDetails services from Microsoft (available in .NET 7+)
            services.AddProblemDetails(problemOptions =>
            {
                // Configure Microsoft's problem details service
                problemOptions.CustomizeProblemDetails = context =>
                {
                    // Add trace ID if configured
                    if (options.IncludeTraceId && !context.ProblemDetails.Extensions.ContainsKey(options.TraceIdExtensionName))
                    {
                        context.ProblemDetails.Extensions[options.TraceIdExtensionName] = context.HttpContext.TraceIdentifier;
                    }
                };
            });

            return services;
        }

        /// <summary>
        /// Adds exception mapping services to the service collection.
        /// Registers the ExceptionMapperRegistry with default exception mappers.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configure">Optional configuration action.</param>
        /// <returns>The service collection for method chaining.</returns>
        public static IServiceCollection AddExceptionMapping(
            this IServiceCollection services,
            Action<ResultKitOptions>? configure = null)
        {
            return AddResultKitProblemDetails(services, configure);
        }

        /// <summary>
        /// Registers a custom exception mapper.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="mapper">The exception mapper to register.</param>
        /// <returns>The service collection for method chaining.</returns>
        public static IServiceCollection AddExceptionMapper(
            this IServiceCollection services,
            IExceptionMapper mapper)
        {
            services.Configure<ResultKitOptions>(options =>
            {
                // This will be executed after the registry is created
            });

            services.PostConfigure<ExceptionMapperRegistry>(registry =>
            {
                registry.Register(mapper);
            });

            return services;
        }
    }
}
