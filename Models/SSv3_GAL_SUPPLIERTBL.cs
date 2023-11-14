using System.ComponentModel.DataAnnotations;


namespace ShopSuppliesV3.Models
{
    public class SSv3_GAL_SUPPLIERTBL
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string SupplierName { get; set; }

        [Required]
        public string AddedByUserName { get; set; }

        public string? EditByUserName { get; set; }

        public string? DeleteByUserName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date Created")]
        public DateTime DateAdded { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date Edited")]
        public DateTime? DateEdited { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date Edited")]
        public DateTime? DateDeleted { get; set; }

        public int isActive { get; set; }
    }
}
