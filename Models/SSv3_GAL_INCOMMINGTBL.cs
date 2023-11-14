using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopSuppliesV3.Models
{
    public class SSv3_GAL_INCOMMINGTBL
    {
        [Key]
        public int Id { get; set; }

        
        public int isActive { get; set; }

        //[Required]
        //[Display(Name = "Category")]
        //public string category { get; set; }

        [Required]
        [Display(Name = "PO Number")]
        public string PONumber { get; set; }

        [Required]
        [Display(Name = "Item Name")]
        public int ItemId { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.####}")]
        [Column(TypeName = "decimal(18,4)")]
        [Display(Name = "Incomming Quantity")]
        public decimal IncommingQuantity { get; set; }

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
        public decimal TotalPrice
        {
            get
            {
                decimal Total = IncommingQuantity * UnitPrice;
                return Total;
            }
            set
            {
            }
        }

        //[Required]
        //[Display(Name = "Area")]
        //public string AreaName { get; set; }

        [Display(Name = "Added By")]
        public string addedBy { get; set; }

        [Display(Name = "Status")]
        public string? Status { get; set; } // new, expired, consumed, full rework, partial rework

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Expiration Date")]
        public DateTime? ExpirationDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Created Date")]
        public DateTime DateAdded { get; set; }

        public string? EditByUserName { get; set; }

        public string? DeleteByUserName { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date Edited")]
        public DateTime? DateEdited { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date Edited")]
        public DateTime? DateDeleted { get; set; }

        //[Display(Name = "Reason For Hold Status")]
        //public string? HoldReason { get; set; }

    }
}
