using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopSuppliesV3.Models
{
    public class SSv3_GAL_APPROVALSTBL //Table that consolodate each request made (Request's OrderNumbers are consolodated here via comma separated values of requestId)
    {
        [Key]
        public int Id { get; set; }

        public string RequestorUsername { get; set; }

        public string ApproverUserName { get; set; }

        public string OrderNumbers { get; set; }

        public DateTime RequestedDate { get; set; }

        public string ForApprovalStatus { get; set; }

        public int IsAdvanceOder { get; set; } //To check weather the request is an advance order (Advance order has expiration of 2 days from date of request)

        [NotMapped]
        public string ForApprovalId
        {
            get
            {
                string format = Id.ToString() + "-" + RequestedDate.ToString("yyyyMMdd");
                return format;
            }
        }

        //THE forApprovalId format is Id + DateRequested(yyyyMMdd)
    }
}
