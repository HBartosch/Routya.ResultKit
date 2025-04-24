using System;
using System.ComponentModel.DataAnnotations;

namespace Routya.ResultKit.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class RequiredIfEmptyAttribute : ValidationAttribute
    {
        private readonly string _otherProperty;

        public RequiredIfEmptyAttribute(string otherProperty)
        {
            _otherProperty = otherProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var prop = validationContext.ObjectType.GetProperty(_otherProperty);
            if (prop == null) return new ValidationResult($"Property '{_otherProperty}' not found.");

            var otherValue = prop.GetValue(validationContext.ObjectInstance);

            bool isOtherEmpty = otherValue == null || string.IsNullOrWhiteSpace(otherValue.ToString());
            bool isCurrentEmpty = value == null || string.IsNullOrWhiteSpace(value.ToString());

            if (isOtherEmpty && isCurrentEmpty)
            {
                return new ValidationResult($"{validationContext.DisplayName} is required when {_otherProperty} is empty.",
                    new[] { validationContext.MemberName ?? validationContext.DisplayName });
            }

            return ValidationResult.Success;
        }
    }
}