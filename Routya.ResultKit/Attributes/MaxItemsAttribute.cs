using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Routya.ResultKit.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class MaxItemsAttribute : ValidationAttribute
    {
        private readonly int _max;

        public MaxItemsAttribute(int max) => _max = max;

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is ICollection collection && collection.Count > _max)
            {
                return new ValidationResult($"{validationContext.DisplayName} must have at most {_max} item(s).",
                    new[] { validationContext.MemberName ?? validationContext.DisplayName });
            }

            return ValidationResult.Success;
        }
    }
}