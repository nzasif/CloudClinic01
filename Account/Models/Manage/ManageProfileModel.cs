using CloudClinic.Shared.Validators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Account.Models.Manage
{
    public class RemoveAccountModel
    {
        [Required, MinLength(4)]
        public string Password { get; set; }
    }
    public class ProfileUpdateModel
    {
        [Required]
        public string FullName { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        [ValidateDOB]
        public DateTime DOB { get; set; }
        [Required]
        [ValidateGender]
        public string Gender { get; set; }
        [Required]
        public string Street { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Province { get; set; }
    }

    public class ProfileViewModel
    {
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string DOB { get; set; }
        public string Gender { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string profilePicUri { get; set; }
        public string RegDate { get; set; }
        public string UpdateDate { get; set; }
    }
/////////////////////////////////////

    public class ChangeUserNameModel
    {
        [Required]
        public string NewUserName { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class ChangeEmailModel
    {
        [Required]
        [EmailAddress]
        public string NewEmail { get; set; }
        [Required]
        public string Password { get; set; }

    }

    public class ChangePasswordModel
    {
        [Required]
        public string NewPassword { get; set; }
        [Required]
        public string OldPassword { get; set; }
    }
///////////////////////////////////////////////////////////////////

    public class ChangeFullNameModel
    {
        [Required]
        public string NewFullName { get; set; }
    }

    public class ChangePhoneNumberModel
    {
        [Required]
        public string NewPhoneNumber { get; set; }
    }


    public class ChangeCityModel
    {
        [Required]
        public string NewCity { get; set; }
    }

    public class ChangeProvinceModel
    {
        [Required]
        public string NewProvince { get; set; }
    }

    public class ChangeStreetModel
    {
        [Required]
        public string NewStreet { get; set; }
    }
    
    public class ChangeGenderModel
    {
        [Required]
        [ValidateGender]
        public string NewGender { get; set; }
    }

    public class ChangeDOBModel
    {
        [Required]
        [ValidateDOB]
        public DateTime NewDOB { get; set; }
    }
}
