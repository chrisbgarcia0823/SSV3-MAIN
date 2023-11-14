using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopSuppliesV3.Models
{
    public class SSv3_ITEM_REQUESTTBL
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DisplayName("Request Category")]
        public string RequestCategory { get; set; }

        public string? OrderNumber { get; set; }
        //{
        //    get
        //    {
        //        string date = DateTime.Now.ToString("MM-yyyy").Remove(2,3);
        //        string id = Id.ToString("000000");
        //        return ($"{date}-{id}");
        //    }
        //    set
        //    {
        //    }
        //}

        //[NotMapped]
        //public int HistoryId
        //{
        //    get
        //    {
        //        int id = int.Parse(OrderNumber.ToLower().Replace("pout-", ""));
        //        return id;
        //    }
        //}

        public int isActive { get; set; }

        [DisplayName("Area")]
        [Required]
        public string AreaName { get; set; }

        [DisplayName("BU")]
        [Required]
        public string BUname { get; set; }

        [DisplayName("Status")]
        [Required]
        public string RequestStatus { get; set; } //new, approve, pending for approval, declined, etc.

        [DisplayName("Requestor")]
        [Required]
        public int RequestorUserId { get; set; }

        [DisplayName("Inventory Item Id")]
        [Required]
        public int InventoryItemId { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.####}")]
        [Column(TypeName = "decimal(18,4)")]
        [DisplayName("Quantity Requested")]
        [Required]
        public decimal QuantityRequested { get; set; }

        ////REMOVE HERE AND PUT IN OUTGOING TABLE
        //[DisplayFormat(DataFormatString = "{0:0.####}")]
        //[Column(TypeName = "decimal(18,2)")]
        //[Display(Name = "Unit Price")]
        //public decimal? UnitPrice { get; set; }

        ////REMOVE HERE AND PUT IN OUTGOING TABLE
        //[DisplayFormat(DataFormatString = "{0:0.##}")]
        //[Column(TypeName = "decimal(18,2)")]
        //[Display(Name = "Unit Price")]
        //public decimal? TotalPrice
        //{
        //    get
        //    {
        //        decimal? Total = QuantityRequested * UnitPrice;
        //        return Total;
        //    }
        //    set
        //    {
        //    }
        //}

        [Required]
        [DisplayName("Purpose")]
        public string Purpose { get; set; }

        //[DisplayName("Approver")]
        //[Required]
        //public int? ApproverUserId { get; set; }

        [DisplayName("Date Requested")]
        [DataType(DataType.DateTime)]
        public DateTime DateRequested { get; set; }

        [DisplayName("Date Requested")]
        [DataType(DataType.DateTime)]
        public DateTime? DateApproved { get; set; }

        [DisplayName("Approver")]
        public int? ApproverUserId { get; set; }

        [DisplayName("isAdvance")]
        public int? isAdvance { get; set; }

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
