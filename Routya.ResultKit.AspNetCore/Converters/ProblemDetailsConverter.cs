using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Routya.ResultKit.AspNetCore.Converters
{
    /// <summary>
    /// Converts between Routya.ResultKit.ProblemDetails and Microsoft.AspNetCore.Mvc.ProblemDetails.
    /// </summary>
    public static class ProblemDetailsConverter
    {
        /// <summary>
        /// Converts a Routya.ResultKit.ProblemDetails instance to Microsoft.AspNetCore.Mvc.ProblemDetails.
        /// </summary>
        /// <param name="source">The source ProblemDetails from Routya.ResultKit.</param>
        /// <returns>A Microsoft.AspNetCore.Mvc.ProblemDetails instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
        public static Microsoft.AspNetCore.Mvc.ProblemDetails ToMicrosoft(Routya.ResultKit.ProblemDetails source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var result = new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Type = source.Type,
                Title = source.Title,
                Status = source.Status,
                Detail = source.Detail,
                Instance = source.Instance
            };

            // Copy all extension members to the Microsoft ProblemDetails Extensions dictionary
            var extensions = source.GetExtensions();
            foreach (var kvp in extensions)
            {
                result.Extensions[kvp.Key] = kvp.Value;
            }

            return result;
        }

        /// <summary>
        /// Converts a Microsoft.AspNetCore.Mvc.ProblemDetails instance to Routya.ResultKit.ProblemDetails.
        /// </summary>
        /// <param name="source">The source ProblemDetails from Microsoft.AspNetCore.Mvc.</param>
        /// <returns>A Routya.ResultKit.ProblemDetails instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
        public static Routya.ResultKit.ProblemDetails FromMicrosoft(Microsoft.AspNetCore.Mvc.ProblemDetails source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var result = new Routya.ResultKit.ProblemDetails
            {
                Type = source.Type ?? "about:blank",
                Title = source.Title,
                Status = source.Status,
                Detail = source.Detail,
                Instance = source.Instance
            };

            // Copy all extension members from Microsoft ProblemDetails Extensions dictionary
            if (source.Extensions != null)
            {
                foreach (var kvp in source.Extensions)
                {
                    result.SetExtension(kvp.Key, kvp.Value);
                }
            }

            return result;
        }
    }
}
