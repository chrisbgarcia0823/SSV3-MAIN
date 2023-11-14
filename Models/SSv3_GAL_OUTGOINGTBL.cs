using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopSuppliesV3.Models
{
    public class SSv3_GAL_OUTGOINGTBL
    {
        [Key]
        public int Id { get; set; }

        public int isActive { get; set; }

        [Required]
        [DisplayName("Request Id")]
        public int RequestId { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:0.####}")]
        [DisplayName("Quantity Issued")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal QuantityIssued { get; set; }


        [DisplayName("Issued from Id")]
        public int? IncommingIds { get; set; }

        //ADDED HERE REMOVE FROM REQUEST TABLE
        [Required]
        [DisplayName("Unit Cost")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitCost { get; set; }

        //ADDED HERE REMOVE FROM REQUEST TABLE
        [DisplayName("Total Cost")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCost { get; set; }

        //[DisplayName("Total Price")]
        //public float TotalPrice { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Date Issued")]
        public DateTime DateIssued { get; set; }

        [Required]
        [DisplayName("Issued By")]
        public string IssuerUsername { get; set; }

        [DisplayName("Remarks")]
        public string Remarks { get; set; }

    }
}
