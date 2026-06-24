using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Shared.Validators
{
    public class ValidateGender : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            switch (value.ToString())
            {
                case "male":
                    return ValidationResult.Success;
                case "female":
                    return ValidationResult.Success;
                case "other":
                    return ValidationResult.Success;
                default:
                    return new ValidationResult("Gender value is not valide");
            }
        }
    }
}
