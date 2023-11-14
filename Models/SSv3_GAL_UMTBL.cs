using System.ComponentModel.DataAnnotations;

namespace ShopSuppliesV3.Models
{
    public class SSv3_GAL_UMTBL
    {
        [Key]
        public int Id { get; set; }

        public string UnitOfMeasure { get; set; }

        public string AddedBy { get; set; }

        public string? EditByUserName { get; set; }

        public string? DeleteByUserName { get; set; }

        public int isActive { get; set; }

        [Required]
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
