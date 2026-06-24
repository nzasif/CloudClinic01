using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Shared.Validators
{
    public class ValidateDrPracTime : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string time = value.ToString();

            switch (time)
            {
                case "f":
                    return ValidationResult.Success;
                case "s":
                    return ValidationResult.Success;
                case "t":
                    return ValidationResult.Success;
                default:
                    return new ValidationResult("Not a valid Dr. practice time");
            }
        }
    }
}