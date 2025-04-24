using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Routya.ResultKit.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class MatchRegexAttribute : ValidationAttribute
    {
        public string Pattern { get; }

        public MatchRegexAttribute(string pattern)
        {
            Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var stringValue = value as string;
            if (stringValue == null)
                return ValidationResult.Success;

            if (!Regex.IsMatch(stringValue, Pattern))
                return new ValidationResult(
                    ErrorMessage ?? $"{validationContext.DisplayName} is not in a valid format.",
                    new[] { validationContext.MemberName ?? validationContext.DisplayName });

            return ValidationResult.Success;
        }
    }
}