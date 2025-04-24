using System;
using System.ComponentModel.DataAnnotations;

namespace Routya.ResultKit.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class ValidDateTimeRangeAttribute : ValidationAttribute
    {
        private readonly string _endDateProperty;

        public ValidDateTimeRangeAttribute(string endDateProperty)
        {
            _endDateProperty = endDateProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var start = value as DateTime?;
            var prop = validationContext.ObjectType.GetProperty(_endDateProperty);
            if (prop == null) return new ValidationResult($"Property '{_endDateProperty}' not found.");

            var end = prop.GetValue(validationContext.ObjectInstance) as DateTime?;

            if (start == null || end == null) return ValidationResult.Success;

            return start <= end
                ? ValidationResult.Success
                : new ValidationResult($"{validationContext.DisplayName} must be earlier than {_endDateProperty}.",
                    new[] { validationContext.MemberName ?? validationContext.DisplayName });
        }
    }
}
