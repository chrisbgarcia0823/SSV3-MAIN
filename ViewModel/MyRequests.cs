using System.ComponentModel.DataAnnotations.Schema;

namespace ShopSuppliesV3.ViewModel
{
    [NotMapped]
    public class MyRequests
    {
        public string DateRequested { get; set; }

        public string DateApproved { get; set; }

        public string DateIssued { get; set; }
    }
}
