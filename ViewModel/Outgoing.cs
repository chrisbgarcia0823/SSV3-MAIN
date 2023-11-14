using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ShopSuppliesV3.ViewModel
{
    [NotMapped]
    public class Outgoing
    {
        [Key]
        public int Id { get; set; }

        public int RequestId { get; set; }

        [DisplayName("Request Category")]
        public string RequestCategory { get; set; }


        [DisplayName("Issued from Id")]
        public int? IncommingId { get; set; }

        [Required]
        [DisplayName("Issued from Id")]
        public decimal? IncommingRemainingQty { get; set; }

        [DisplayName("Requested By")]
        public string RequestedByName { get; set; }

        [DisplayName("Approverd By")]
        public string ApproverName { get; set; }

        [Required]
        [DisplayName("Item Requested")]
        public int ItemId { get; set; }

        //[DisplayName("Item Name")]
        //public string ItemName { get; set; }

        [DisplayName("Item Number")]
        public string ItemNumber { get; set; }

        [DisplayName("Item Name")]
        public string RequestStatus { get; set; }

        [DisplayName("Description")]
        public string Description { get; set; }

        [DisplayName("Quantity Requested")]
        public decimal QuantityRequested { get; set; }

        [DisplayName("Quantity Issued")]
        public decimal QuantityIssued { get; set; }

        [DisplayName("UM")]
        public string UnitOfMeasure { get; set; }

        //[DisplayFormat(DataFormatString = "{0:0.###}")]
        //[Column(TypeName = "decimal(18,2)")]
        //[DisplayName("Unit Price")]
        //public decimal UnitPrice { get; set; }

        //[DisplayFormat(DataFormatString = "{0:0.###}")]
        //[Column(TypeName = "decimal(18,2)")]
        //[DisplayName("Total Price")]
        //public decimal TotalPrice { get; set; }

        [DisplayName("Purpose")]
        public string Purpose { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Date Requested")]
        public DateTime DateRequested { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Date Issued")]
        public DateTime DateIssued { get; set; }

        [Required]
        [DisplayName("Issued By")]
        public string IssuedByName { get; set; }

        //ADDED HERE REMOVE FROM REQUEST TABLE
        [DisplayName("Unit Cost")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitCost { get; set; }

        //ADDED HERE REMOVE FROM REQUEST TABLE
        [DisplayName("Total Cost")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCost  { get; set; }

        [DisplayName("Area")]
        [Required]
        public string AreaName { get; set; }

        public string? OrderNumber { get; set; }

        [DisplayName("Remarks")]
        public string OutgoingRemarks { get; set; }

        public string IncomingPOnumber { get; set; }
    }
}
