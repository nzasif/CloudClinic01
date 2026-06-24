using CloudClinic.Shared.Validators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Doctor.Models.PharmacyModels
{
    public class DrugUpdateModel
    {
        [Required]
        public Guid DrugId { get; set; }
        [Required]
        public string DrugName { get; set; }
        [Required]
        public string DrugType { get; set; }
        public string DrugFormula { get; set; }
        public string DrugCompany { get; set; }
        [Range(1, 500000)]
        public int DrugPurchasePrice { get; set; }
        [Range(1, 600000)]
        public int DrugSalePrice { get; set; }

        [ValidateDrugExpDate]
        [DataType(DataType.Date)]
        public DateTime DrugExpiryDate { get; set; }
        [Range(1, 1000)]
        public int DrugAvailQuantity { get; set; }

        [RegularExpression("[\\d]+\\s?(?:mg|ml)", ErrorMessage = "invalid formate")]
        public string DrugWeight { get; set; }
    }
}
