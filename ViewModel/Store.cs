using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopSuppliesV3.ViewModel
{
    [NotMapped]
    public class Store
    {
        [Key]
        public int Id { get; set; }

        [DisplayName]
        public int InventoryId { get; set; }

        [DisplayName("Item Id")]
        public int ItemId { get; set; }

        [DisplayName("Item BU")]
        public string ItemBU { get; set; }

        [DisplayName("In Request")]
        public decimal InRequestStocks { get; set; }

        [DisplayName("Requestor")]
        public string UserName { get; set; }

        [DisplayName("Item Number")]
        public string ItemNumber { get; set; }

        [DisplayName("Description")]
        public string Description { get; set; }

        [DisplayName("Supplier")]
        public string Supplier { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.####}")]
        [Column(TypeName = "decimal(18,4)")]
        [DisplayName("Remaining Stocks")]
        public decimal RemainingStocksOnHand { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.####}")]
        [Column(TypeName = "decimal(18,4)")]
        [Display(Name = "On Hold Quantity")]
        public decimal TotalOnHoldQuantity { get; set; } //FOR HOLD ITEMS

        [DisplayFormat(DataFormatString = "{0:0.####}")]
        [Column(TypeName = "decimal(18,4)")]
        [DisplayName("Remaining Stocks")]
        public decimal RemainingStocksToRequest { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.####}")]
        [Column(TypeName = "decimal(18,4)")]
        [DisplayName("Issued Stocks")]
        public decimal OutgoingStocks { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.####}")]
        [Column(TypeName = "decimal(18,4)")]
        [DisplayName("Remaining Stocks")]
        public decimal Adjustment { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.####}")]
        [Column(TypeName = "decimal(18,4)")]
        [Required]
        [DisplayName("Safety Stock")]
        public decimal SafetyStock { get; set; }

        [DisplayName("Quantity Requested")]
        public int QuantityRequested { get; set; }

        [DisplayName("UM")]
        public string UnitOfMeasure { get; set; }

        [DisplayName("Purpose")]
        public string Purpose { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Requested Date")]
        public DateTime DateRequested { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Item Expiration Date")]
        public DateTime? ItemExpirationDate { get; set; }
    }
}
