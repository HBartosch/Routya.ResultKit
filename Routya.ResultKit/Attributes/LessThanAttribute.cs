#nullable enable
using System;
using System.ComponentModel.DataAnnotations;

namespace Routya.ResultKit.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class LessThanAttribute : ValidationAttribute
    {
        private readonly string _otherProperty;

        public LessThanAttribute(string otherProperty)
        {
            _otherProperty = otherProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;

            var thisValue = Convert.ToDecimal(value);
            var property = validationContext.ObjectType.GetProperty(_otherProperty);

            if (property == null)
                return new ValidationResult($"Property '{_otherProperty}' not found.");

            var otherValue = Convert.ToDecimal(property.GetValue(validationContext.ObjectInstance)!);

            if (thisValue >= otherValue)
                return new ValidationResult($"{validationContext.DisplayName} must be less than {_otherProperty}",
                    new[] { validationContext.MemberName ?? validationContext.DisplayName });

            return ValidationResult.Success;
        }
    }
}