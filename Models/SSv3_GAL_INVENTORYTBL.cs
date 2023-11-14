//using Microsoft.EntityFrameworkCore.Metadata.Internal;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace ShopSuppliesV3.Models
//{
//    public class SSv3_GAL_INVENTORYTBL
//    {
//        [Key]
//        public int Id { get; set; }

//        public int IncommingItemId { get; set; }

//        [DisplayFormat(DataFormatString = "{0:0.####}")]
//        [Column(TypeName = "decimal(18,4)")]
//        public decimal IncommingTotalQuantity { get; set; }

//        [DisplayFormat(DataFormatString = "{0:0.####}")]
//        [Column(TypeName = "decimal(18,4)")]
//        public decimal InRequestTotalQuantity { get; set; }

//        [DisplayFormat(DataFormatString = "{0:0.####}")]
//        [Column(TypeName = "decimal(18,4)")]
//        public decimal OutgoingTotalQuantity { get; set; }

//        [DisplayFormat(DataFormatString = "{0:0.####}")]
//        [Column(TypeName = "decimal(18,4)")]
//        public decimal RemainingStocksToRequest { get; set; }

//        [DisplayFormat(DataFormatString = "{0:0.####}")]
//        [Column(TypeName = "decimal(18,4)")]
//        public decimal RemainingStocksOnhand { get; set; }
//    }
//}


using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopSuppliesV3.Models
{
    public class SSv3_GAL_INVENTORYTBL
    {
        [Key]
        public int Id { get; set; }

        public int IncommingItemId { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.####}")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal IncommingTotalQuantity { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.####}")]
        [Column(TypeName = "decimal(18,4)")]
        [Display(Name = "On Hold Quantity")]
        public decimal TotalOnHoldQuantity { get; set; } //FOR HOLD ITEMS

        [DisplayFormat(DataFormatString = "{0:0.####}")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal InRequestTotalQuantity { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.####}")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal OutgoingTotalQuantity { get; set; }

        //ADDED AJUSTMENT COLUMN TO ADJUST THE ACTUAL INVENTORY
        [DisplayFormat(DataFormatString = "{0:0.####}")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal Adjustment { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.####}")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal RemainingStocksToRequest
        {
            get
            {
                return (IncommingTotalQuantity - (OutgoingTotalQuantity + InRequestTotalQuantity + TotalOnHoldQuantity) + (Adjustment));
            }
            set
            {
            }
        }

        [DisplayFormat(DataFormatString = "{0:0.####}")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal RemainingStocksOnhand
        {
            get
            {
                return (IncommingTotalQuantity - (OutgoingTotalQuantity + TotalOnHoldQuantity) + (Adjustment));
            }
            set
            {

            }
        }
    }
}