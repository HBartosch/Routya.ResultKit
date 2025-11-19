#nullable enable
using Routya.ResultKit.Validation.Constants;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Routya.ResultKit
{
    public static class ValidationExtensions
    {
        public static Result<T> Validate<T>(this T obj)
        {
            var results = new List<ValidationResult>();
            var visited = new HashSet<object>();
            CollectValidationResults(obj, results, visited, prefix: null);

            if (results.Count == 0)
                return Result<T>.Ok(obj);

            var errors = results
                .SelectMany(r =>
                    (r.MemberNames.Any() ? r.MemberNames : new[] { ValidationConstants.DefaultMemberName })
                    .Select(member => new { Member = member, Error = r.ErrorMessage ?? ValidationConstants.DefaultErrorMessage }))
                .GroupBy(x => x.Member)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.Error).ToArray());

            return Result<T>.ValidationFailed(errors);
        }

        private static void CollectValidationResults(object obj, List<ValidationResult> results, HashSet<object> visited, string? prefix)
        {
            if (obj == null || visited.Contains(obj))
                return;

            visited.Add(obj);
            var context = new ValidationContext(obj);
            var tempResults = new List<ValidationResult>();
            Validator.TryValidateObject(obj, context, tempResults, validateAllProperties: true);

            foreach (var result in tempResults)
            {
                var memberNames = result.MemberNames.Any()
                    ? result.MemberNames.Select(name => prefix != null ? $"{prefix}.{name}" : name)
                    : new[] { prefix ?? ValidationConstants.DefaultMemberName };

                results.Add(new ValidationResult(result.ErrorMessage, memberNames));
            }

            var nestedProps = obj.GetType().GetProperties()
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
                .Where(p => p.PropertyType.IsClass && p.PropertyType != typeof(string));

            foreach (var prop in nestedProps)
            {
                var value = prop.GetValue(obj);
                var nestedPrefix = prefix != null ? $"{prefix}.{prop.Name}" : prop.Name;
                CollectValidationResults(value, results, visited, nestedPrefix);
            }
        }
    }
}