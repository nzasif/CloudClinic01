using CloudClinic.Shared.Validators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Doctor.Models.PharmacyModels
{
    public class DrugViewModel
    {
        public Guid DrugId { get; set; }
        public string DrugName { get; set; }
        public string DrugType { get; set; }
        public string DrugFormula { get; set; }
        public int DrugPurchasePrice { get; set; }
        public int DrugSalePrice { get; set; }
        public string DrugExpiryDate { get; set; }
        public string DrugCompany { get; set; }
        public int DrugAvailQuantity { get; set; }
        public string DrugWeight { get; set; }
    }
}
