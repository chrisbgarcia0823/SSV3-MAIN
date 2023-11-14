using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace ShopSuppliesV3.ViewModel
{
    [NotMapped]
    public class PullHistory
    {
        public int Id { get; set; }

        public string ItemNumber { get; set; }

        public string Status { get; set; }

        public string BUname { get; set; }

        public string UnitOfMeasure { get; set; }

        public string OrderNumber { get; set; }

        public string Action { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.####}")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal PullQuantity { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.####}")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal UnitPrice { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.##}")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Unit Price")]
        public decimal TotalPrice { get; set; }

        public string Reason { get; set; }

        public string TransactedBy { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime TransactionDate { get; set; }


    }
}
