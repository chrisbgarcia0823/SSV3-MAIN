using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ShopSuppliesV3.Helpers;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopSuppliesV3.ViewModel
{
    [NotMapped]
    public class Requests
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DisplayName("Request Category")]
        public string RequestCategory { get; set; }

        [NotMapped]
        public string OrderNumber //{ get; set; }
        {
            get
            {
                string date = DateTime.Now.ToString("MM-yyyy").Remove(2, 3);
                string id = Id.ToString("000000");
                return ($"{date}-{id}");
            }
            set
            {
            }
        }

        public int isActive { get; set; }

        [DisplayName("BU")]
        [Required]
        public string BUname { get; set; }

        [DisplayName("Incomming Id")]
        [Required]
        public int IncommingId { get; set; }

        [DisplayName("Area")]
        [Required]
        public string AreaName { get; set; }

        [DisplayName("Status")]
        [Required]
        public string RequestStatus { get; set; } //approve, pending for approval, declined, etc.

        [DisplayName("Requestor ID")]
        [Required]
        public int RequestorUserId { get; set; }

        [DisplayName("Requestor")]
        [Required]
        public string RequestorName { get; set; }

        [DisplayName("Item Id")]
        [Required]
        public int ItemId { get; set; }

        [DisplayName("Item Number")]
        [Required]
        public string ItemNumber { get; set; }

        [DisplayName("Description")]
        [Required]
        public string ItemDescription { get; set; }

        [DisplayName("Unit of Measure")]
        [Required]
        public string UnitOfMeasure { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.####}")]
        [Column(TypeName = "decimal(18,4)")]
        [DisplayName("Quantity Requested")]
        [Required]
        public decimal QuantityRequested { get; set; }


        //REMOVE HERE AND PUT IN OUTGOING TABLE
        [DisplayFormat(DataFormatString = "{0:0.####}")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Unit Cost")]
        public decimal? UnitCost { get; set; }

        //REMOVE HERE AND PUT IN OUTGOING TABLE
        [DisplayFormat(DataFormatString = "{0:0.####}")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Cost")]
        public decimal? TotalCost { get; set; }


        [DisplayFormat(DataFormatString = "{0:0.####}")]
        [Column(TypeName = "decimal(18,4)")]
        [DisplayName("Quantity Issued")]
        [Required]
        public decimal QuantityIssued { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.####}")]
        [Column(TypeName = "decimal(18,4)")]
        [DisplayName("Remaining Quantity to Request")]
        [Required]
        public decimal RemainingQuntityToRequest { get; set; }

        [Required]
        [DisplayName("Purpose")]
        public string Purpose { get; set; }

        [DisplayName("Approver USN")]
        [Required]
        public string ApproverUsername { get; set; }

        [DisplayName("Requestor Name")]
        [Required]
        public string ApproverName { get; set; }

        [DisplayName("Date Requested")]
        [DataType(DataType.DateTime)]
        public DateTime DateRequested { get; set; }

        [DisplayName("Date Approved")]
        [DataType(DataType.DateTime)]
        public DateTime? DateApproved { get; set; }

        [DisplayName("Date Issued")]
        [DataType(DataType.DateTime)]
        public DateTime DateIssued { get; set; }

        [DisplayName("PO Number")]
        public string PONumber { get; set; }

        public string IssuerName { get; set; }

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

    }
}
