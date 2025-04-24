using System;
using System.ComponentModel.DataAnnotations;

namespace Routya.ResultKit.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class RequiredIfAttribute : ValidationAttribute
    {
        private readonly string _property;
        private readonly object _expectedValue;

        public RequiredIfAttribute(string property, object expectedValue)
        {
            _property = property;
            _expectedValue = expectedValue;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var prop = validationContext.ObjectType.GetProperty(_property);
            if (prop == null) return new ValidationResult($"Property '{_property}' not found.");

            var actual = prop.GetValue(validationContext.ObjectInstance);
            if (Equals(actual, _expectedValue) && (value == null || string.IsNullOrWhiteSpace(value.ToString())))
                return new ValidationResult($"{validationContext.DisplayName} is required when {_property} is {_expectedValue}.",
                    new[] { validationContext.MemberName ?? validationContext.DisplayName });

            return ValidationResult.Success;
        }
    }
}