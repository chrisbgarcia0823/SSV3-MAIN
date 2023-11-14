using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopSuppliesV3.ViewModel
{
    [NotMapped]
    public class Cart
    {
        [Key]
        public int RequestorUserId { get; set; }

        public int InventoryItemId { get; set; }

        public string RequestorName { get; set; }

        public int CountItemRequested { get; set; }

        public string Status { get; set; }
    }
}
