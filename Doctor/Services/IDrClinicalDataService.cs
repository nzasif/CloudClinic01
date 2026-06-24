using CloudClinic.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Doctor.Services
{
    public interface IDrClinicalDataService
    {
        int DecDrugAvailQuantity(CloudClinicDb db, string drId, string drugId, int drugQuantity);
        int IncDrugAvailQuantity(CloudClinicDb db, string drId, string drugId, int DrugQuantity);

    }
}
