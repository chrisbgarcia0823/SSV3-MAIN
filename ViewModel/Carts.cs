using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopSuppliesV3.ViewModel
{
    [NotMapped]
    public class Carts
    {
        [Key]
        public int RequestorId { get; set; }

        public string BUname { get; set; }

        public string RequestorName { get; set; }

        public int RequestCount { get; set; }

        public int ApproverUserId { get; set; }

        public string ApproverName { get; set; }
    }
}
