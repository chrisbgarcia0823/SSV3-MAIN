using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopSuppliesV3.Models
{
    public class SSv3_GAL_INCOMING_HISTORYTBL
    {
        [Key]
        public int Id { get; set; }

        public string? OrderNumber { get; set; }

        public int IncomingId { get; set; }

        public string Action { get; set; } //pullout, pullin

        [DisplayFormat(DataFormatString = "{0:0.####}")]
        [Column(TypeName = "decimal(18,4)")]
        [Display(Name = "Quantity")]
        public decimal Quantity{ get; set; }

        [DisplayFormat(DataFormatString = "{0:0.####}")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Unit Price")]
        public decimal UnitPrice { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.##}")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Unit Price")]
        public decimal TotalPrice
        {
            get
            {
                decimal Total = Quantity * UnitPrice;
                return Total;
            }
            set
            {
            }
        }

        public string Reason { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Pullout Date")]
        public DateTime DateAdded { get; set; }

        public string AddedByUsername { get; set; }

        public string? Status { get; set; }

        public string POnumber { get; set; }
    }
}
