using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace ShopSuppliesV3.ViewModel
{
    [NotMapped]
    public class BU
    {
        [Key]
        public int Id { get; set; }

        public int isActive { get; set; }

        [Display(Name = "BU")]
        public string BUname { get; set; }

        public string AddedByUserName { get; set; }

        public string? EditByUserName { get; set; }

        public string? DeleteByUserName { get; set; }

        public string AddedByName { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date Added")]
        public DateTime DateAdded { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date Edited")]
        public DateTime? DateEdited { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date Edited")]
        public DateTime? DateDeleted { get; set; }

    }
}
