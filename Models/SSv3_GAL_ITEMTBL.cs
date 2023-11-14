using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopSuppliesV3.Models
{
    public class SSv3_GAL_ITEMTBL
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "BU")]
        public string BUName { get; set; }

        //[Required]
        //[Display(Name = "Item Name")]
        //public string ItemName { get; set; }

        //[Required]
        //[Display(Name = "Area")]
        //public string AreaName { get; set; }

        [Required]
        [Display(Name = "Item Number")]
        public string ItemNumber { get; set; }

        [Required]
        [DisplayName("Unit of Measure")]
        public string UnitOfMeasure { get; set; }

        [Required]
        [Display(Name = "Supplier")]
        public int SupplierId { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Added By")]
        public string AddedByUsername { get; set; }

        public string? EditByUserName { get; set; }

        public string? DeleteByUserName { get; set; }

        [Display(Name = "Active")]
        public int? isActive { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.####}")]
        [Column(TypeName = "decimal(18,4)")]
        [Required]
        [DisplayName("Safety Stock")]
        public decimal SafetyStock { get; set; }


        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date Created")]
        public DateTime DateAdded { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date Edited")]
        public DateTime? DateEdited { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date Edited")]
        public DateTime? DateDeleted { get; set; }

        [Required]
        [DisplayName("Buyer Name")]
        public string BuyerName { get; set; }

    }
}
