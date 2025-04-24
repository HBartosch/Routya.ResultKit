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
            var context = new ValidationContext(obj);
            var results = new List<ValidationResult>();
            if (Validator.TryValidateObject(obj, context, results, true))
            {
                return Result<T>.Ok(obj);
            }

            var errors = results
                .GroupBy(r => r.MemberNames.FirstOrDefault() ?? ValidationConstants.DefaultMemberName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(r => r.ErrorMessage ?? ValidationConstants.DefaultErrorMessage).ToArray());

            return Result<T>.Fail(ValidationConstants.DefaultTitle, ValidationConstants.DefaultStatusCode, errors);
        }
    }
}