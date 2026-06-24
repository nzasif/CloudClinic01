using CloudClinic.Account.Models;
using CloudClinic.Data;
using CloudClinic.Data.DbSets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Account.Services
{
    public interface IRegisterService
    {
        // this will add initial dr details, like his AppUser Id,
        // and also set the IsVerified attribute to false
        Task<DrDetail> AddInitDrDetial(CloudClinicDb db, string userId);
    }
}
