using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopSuppliesV3.Models
{
    public class SSv3_GAL_ADJUSTMENTTBL
    {
        [Key]
        public int Id { get; set; }

        public string AdjustedBy { get; set; }

        public int InventoryId { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.####}")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Adjustment")]
        public decimal Adjustment { get; set; }


        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date Adjusted")]
        public DateTime AdjustmentDate { get; set; }

        public string Remarks { get; set; }

    }
}
