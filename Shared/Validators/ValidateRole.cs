using CloudClinic.Shared.AuthConstants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Shared.Validators
{
    public class ValidateNonStaffRole : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string role = value.ToString();
            switch (role)
            {
                case Roles.NormalUserRole:
                    return ValidationResult.Success;
                case Roles.DrRole:
                    return ValidationResult.Success;
                default:
                    return new ValidationResult("Invalid Role");
            }
        }
    }

    public class ValidateStaffRole : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            List<string> roles = value as List<string>;

            foreach (var role in roles)
            {
                switch (role)
                {
                    case Roles.DrAccountantRole:
                        break;
                    case Roles.DrAssistantRole:
                        break;
                    case Roles.DrLabStaffRole:
                        break;
                    case Roles.DrXrayStaffRole:
                        break;
                    case Roles.DrPharmacyStaffRole:
                        break;
                    default:
                        return new ValidationResult("Invalid Role");
                }
            }

            return ValidationResult.Success;
        }
    }
}
