using System;
#nullable enable
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Routya.ResultKit.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class MinItemsAttribute : ValidationAttribute
    {
        private readonly int _min;

        public MinItemsAttribute(int min) => _min = min;

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is ICollection collection && collection.Count < _min)
            {
                return new ValidationResult($"{validationContext.DisplayName} must have at least {_min} item(s).",
                    new[] { validationContext.MemberName ?? validationContext.DisplayName });
            }

            return ValidationResult.Success;
        }
    }
}