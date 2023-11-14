using System.ComponentModel.DataAnnotations.Schema;

namespace ShopSuppliesV3.ViewModel
{
    [NotMapped]
    public class OrderSummary
    {
        public string RequestCategory { get; set; }

        public string OrderNumber { get; set; }

        public string ItemNumber { get; set; }

        public decimal QuantityRequested { get; set; }
        
        public string UnitOfMeasure { get; set; }

        public string ItemDescription { get; set; }

        public string Purpose { get; set; }
    }
}
