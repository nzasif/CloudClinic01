using CloudClinic.Shared.Validators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Doctor.Models.PharmacyModels
{
    public class DrugCreateModel
    {
        [Required]
        public string DrugName { get; set; }
        [Required]
        public string DrugType { get; set; }
        public string DrugFormula { get; set; }
        public string DrugCompany { get; set; }
        public int DrugPurchasePrice { get; set; }
        public int DrugSalePrice { get; set; }

        [ValidateDrugExpDate]
        [DataType(DataType.Date)]
        public DateTime DrugExpiryDate { get; set; }
        [Required, Range(1, 1000)]
        public int DrugAvailQuantity { get; set; }
        [RegularExpression("[\\d]+\\s?(?:mg|ml)", ErrorMessage = "invalid formate")]
        public string DrugWeight { get; set; }
    }
}
