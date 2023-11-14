using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ShopSuppliesV3.ViewModel
{
    [NotMapped]
    public class InventoryAdjustments
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string ItemNumber { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal AdjustmentQty { get; set; }

        public DateTime AdjustmentDate { get; set; }

        public string Remarks { get; set; }

    }
}
