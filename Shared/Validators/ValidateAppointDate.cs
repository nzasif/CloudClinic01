using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Shared.Validators
{
    public class ValidateAppointDate : ValidationAttribute
    {
        protected override ValidationResult IsValid(object objValue, ValidationContext validationContext)
        {
            try
            {
                DateTime appointDate = DateTime.Parse(objValue.ToString());
                // DateTime today = DateTime.Now;

                #region
                //if (appointDate.Year < today.Year)
                //{
                //    return new ValidationResult("Past year");
                //}

                //if (appointDate.Month < today.Month)
                //{
                //    return new ValidationResult("Past month");
                //}

                //if (appointDate.Day < today.Day)
                //{
                //    return new ValidationResult("Past day");
                //}
                #endregion
                if (appointDate.Date < DateTime.Now.Date)
                {
                    return new ValidationResult("Invalid date, this is expired..");
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