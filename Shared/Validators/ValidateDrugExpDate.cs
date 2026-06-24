using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Shared.Validators
{
    public class ValidateDrugExpDate : ValidationAttribute
    {
        protected override ValidationResult IsValid(object objValue, ValidationContext validationContext)
        {
            try
            {
                DateTime drugExpDate = DateTime.Parse(objValue.ToString());
                DateTime today = DateTime.Now.Date;

                if (drugExpDate < today)
                {
                    return new ValidationResult("This drug is already expired, or the exp-date is not valid.");
                }
            }
            catch (Exception e)
            {
                return new ValidationResult("Not a valid date");
            }

            return ValidationResult.Success;
        }
    }
}