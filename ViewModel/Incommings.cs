using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace ShopSuppliesV3.ViewModel
{
    [NotMapped]
    public class Incommings
    {
        [Key]
        public int Id { get; set; }

        public int isActive { get; set; }

        //[Required]
        //[Display(Name = "Item Name")]
        //public int ItemId { get; set; }

        [Required]
        [Display(Name = "BU")]
        public string BUName { get; set; }

        [Required]
        [Display(Name = "Item Number")]
        public string ItemNumber { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }

        //[Required]
        //[Display(Name = "Category")]
        //public string category { get; set; }

        [Required]
        [Display(Name = "PO Number")]
        public string PONumber { get; set; }

        [Required]
        [Display(Name = "Supplier Name")]
        public string SupplierName { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.####}")]
        [Column(TypeName = "decimal(18,4)")]
        [Display(Name = "Quantity")]
        public decimal Quantity { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.####}")]
        [Column(TypeName = "decimal(18,4)")]
        [Display(Name = "Outgoing Quantity")]
        public decimal? TotalOutgoingQuantity { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.####}")]
        [Column(TypeName = "decimal(18,4)")]
        [Display(Name = "On Hold Quantity")]
        public decimal TotalOnHoldQuantity { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.####}")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Unit Price")]
        public decimal UnitPrice { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.##}")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Unit Price")]
        public decimal TotalPrice { get; set; }
        //{
        //    get
        //    {
        //        decimal Total = Quantity * UnitPrice;
        //        return Total;
        //    }
        //    set
        //    {

        //    }
        //}

        [Required]
        [Display(Name = "Area")]
        public string AreaName { get; set; }

        [Display(Name = "Added By")]
        public string addedBy { get; set; }

        [Display(Name = "Status")]
        public string? Status { get; set; } // new, expired, consumed, rework, partial rework

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Expiration Date")]
        public DateTime? ExpirationDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Created Date")]
        public DateTime DateAdded { get; set; }

        [Display(Name = "Reason For Hold Status")]
        public string? HoldReason { get; set; }
    }
}
