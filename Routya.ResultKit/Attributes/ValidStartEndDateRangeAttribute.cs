using Routya.ResultKit.Validation.Constants;
using System;
using System.ComponentModel.DataAnnotations;

namespace Routya.ResultKit.Attributes
{

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ValidStartEndDateRangeAttribute : ValidationAttribute
    {
        private readonly string _startDateProperty;
        private readonly string _endDateProperty;

        public ValidStartEndDateRangeAttribute(string startDateProperty, string endDateProperty)
        {
            _startDateProperty = startDateProperty;
            _endDateProperty = endDateProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;

            var type = validationContext.ObjectType;

            var startProp = type.GetProperty(_startDateProperty);
            var endProp = type.GetProperty(_endDateProperty);

            if (startProp == null || endProp == null)
                return new ValidationResult($"Properties '{_startDateProperty}' or '{_endDateProperty}' not found.");

            var startValue = startProp.GetValue(value) as DateTime?;
            var endValue = endProp.GetValue(value) as DateTime?;

            if (!startValue.HasValue || !endValue.HasValue)
                return ValidationResult.Success;

            if (startValue > endValue)
            {
                return new ValidationResult(
                    $"{_startDateProperty} must be earlier than {_endDateProperty}.",
                    new[] { ValidationConstants.DefaultMemberName }
                );
            }

            return ValidationResult.Success;
        }
    }
}
