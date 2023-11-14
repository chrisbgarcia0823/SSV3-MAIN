using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace ShopSuppliesV3.ViewModel
{
    [NotMapped]
    public class Approvals
    {
        [Key]
        public int Id { get; set; }

        public string ForApprovalId
        {
            get
            {
                string format = Id.ToString() + "-" + RequestedDate.ToString("yyyyMMdd");
                return format;
            }
        }

        public string RequestorUsername { get; set; }

        public string RequestorName { get; set; }

        public string ApproverUserName { get; set; }

        public string OrderNumbers { get; set; }

        public DateTime RequestedDate { get; set; }

        public int IsAdvanceOder { get; set; } //To check weather the request is an advance order (Advance order has expiration of 2 days from date of request)

        public string ForApprovalStatus { get; set; }

        //THE forApprovalId format is Id + DateRequested(yyyyMMdd)

        public string RequestCategory { get; set; }

        public string OrderNumber { get; set; }

        public string ItemNumber { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.####}")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Unit Price")]
        public decimal QuantityRequested { get; set; }

        public string UnitOfMeasure { get; set; }

        public string ItemDescription { get; set; }

        public string Purpose { get; set; }

        public int IncomingId { get; set; }
    }
}
