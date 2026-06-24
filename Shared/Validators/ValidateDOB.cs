using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Shared.Validators
{
    public class ValidateDOB : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
			try
			{
				DateTime DOB = DateTime.Parse(value.ToString());

				if (DOB > DateTime.Now.Date)
				{
					return new ValidationResult("Date of birth may not be in future.");
				}

				if ((DateTime.Now.Date.Year - DOB.Date.Year) > 120)
				{
					return new ValidationResult("Date of birth is not valid, you age should not be more than 120 years.");
				}

				return ValidationResult.Success;
			}
			catch (Exception e)
			{
				return new ValidationResult("Not a valid Date");
			}
        }
    }
}
