using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopSuppliesV3.ViewModel
{
    [NotMapped]
    public class Items
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "BU")]
        public string BUName { get; set; }

        //[Required]
        //[DisplayName("Item Name")]
        //public string ItemName { get; set; }

        //[Required]
        //[Display(Name = "Area")]
        //public string AreaName { get; set; }

        [Required]
        [DisplayName("Item Number")]
        public string ItemNumber { get; set; }

        [Required]
        [DisplayName("Unit of Measure")]
        public string UnitOfMeasure { get; set; }

        [Required]
        [DisplayName("Supplier")]
        public string SupplierName { get; set; }

        [Required]
        [DisplayName("Description")]
        public string Description { get; set; }

        [DisplayName("Added By")]
        public string UserName { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.####}")]
        [Column(TypeName = "decimal(18,4)")]
        [Required]
        [DisplayName("Safety Stock")]
        public decimal SafetyStock { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date Created")]
        public DateTime DateAdded { get; set; }

        [Required]
        [DisplayName("Buyer Name")]
        public string BuyerName { get; set; }
    }
}