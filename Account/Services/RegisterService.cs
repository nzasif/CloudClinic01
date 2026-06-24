using CloudClinic.Account.Models;
using CloudClinic.Data;
using CloudClinic.Data.DbSets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Account.Services
{
    public class RegisterService: IRegisterService
    {
        // this will add initial dr details, like his AppUser Id,
        // and also set the IsVerified attribute to false
        public async Task<DrDetail> AddInitDrDetial(CloudClinicDb db, string userId)
        {
            DrDetail drDetail = new DrDetail
            {
                UserId = userId,
                // for development IsVerified = true
                IsVerified = true,
                IsVisible = true
            };

            await db.DrDetails.AddAsync(drDetail);

            //UnVerifiedDr unVerifiedDr = new UnVerifiedDr
            //{
            //    DrId = drDetail.DrId,
            //    UnVerificationCause = "verification is pending",
            //    UnVerificationDate = DateTime.Now
            //};

            //await db.UnVerifiedDrs.AddAsync(unVerifiedDr);
            await db.SaveChangesAsync();
            return drDetail;
        }
    }
}
