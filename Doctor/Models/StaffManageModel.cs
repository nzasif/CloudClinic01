using CloudClinic.Shared.Validators;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Doctor.Models
{
    public class StaffCreateModel
    {
        [Required]
        public string FullName { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        [ValidateDOB]
        public DateTime DOB { get; set; }
        [Required]
        [ValidateGender]
        public string Gender { get; set; }
        [Required]
        public string Province { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Street { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        [ValidateStaffRole]
        public List<string> Roles { get; set; }
        [Required]
        public int StaffSalary { get; set; }
    }

    public class StaffUpdateModel
    {
        [Required]
        public string StaffId { get; set; }

        [Required]
        public string StaffUserId { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        [ValidateDOB]
        public DateTime DOB { get; set; }
        [Required]
        [ValidateGender]
        public string Gender { get; set; }
        [Required]
        public string Province { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Street { get; set; }
        [Required]
        public string Password { get; set; }

        [Required]
        [ValidateStaffRole]
        public List<string> Roles { get; set; }

        [Required]
        public int StaffSalary { get; set; }
        public IFormFile ProfilePic { get; set; }
    }

    public class StaffViewModel
    {
        public string StaffUserId { get; set; }
        public string StaffId { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string DOB { get; set; }
        public string Gender { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string Password { get; set; }
        public List<string> Roles { get; set; }
        public int StaffSalary { get; set; }
        public string ProfilePicUri { get; set; }
        public bool IsRemoved { get; set; }
        public string RegDate { get; set; }
        public string UpdateDate { get; set; }
    }
}
