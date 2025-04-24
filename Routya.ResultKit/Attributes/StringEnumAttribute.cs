using System;
using System.ComponentModel.DataAnnotations;

namespace Routya.ResultKit.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class StringEnumAttribute : ValidationAttribute
    {
        private readonly Type _enumType;

        public StringEnumAttribute(Type enumType)
        {
            _enumType = enumType;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string str && Enum.IsDefined(_enumType, str))
            {
                return ValidationResult.Success;
            }
            return new ValidationResult($"{validationContext.DisplayName} must be one of: {string.Join(", ", Enum.GetNames(_enumType))}",
                    new[] { validationContext.MemberName ?? validationContext.DisplayName });
        }
    }
}