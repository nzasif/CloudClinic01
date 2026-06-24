using CloudClinic.Data;
using CloudClinic.Data.DbSets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Doctor.Services
{
    public class DrClinicalDataService : IDrClinicalDataService
    {
        public int DecDrugAvailQuantity(CloudClinicDb db, string drId, string drugId, int DrugQuantity)
        {
            Drug drug = db.Drugs.Find(new Guid(drugId));

            if (drug == null)
            {
                return -1;
            }

            drug.DrugAvailQuantity = drug.DrugAvailQuantity - DrugQuantity;

            db.SaveChanges();

            return drug.DrugAvailQuantity;
        }
        public int IncDrugAvailQuantity(CloudClinicDb db, string drId, string drugId, int DrugQuantity)
        {
            Drug drug = db.Drugs.Find(new Guid(drugId));

            if (drug == null)
            {
                return -1;
            }

            drug.DrugAvailQuantity = drug.DrugAvailQuantity + DrugQuantity;

            db.SaveChanges();

            return drug.DrugAvailQuantity;
        }
    }
}
