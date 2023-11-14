using System.ComponentModel.DataAnnotations;

namespace ShopSuppliesV3.Models
{
    public class SSv3_GAL_USERTBL
    {
        [Key]
        public int Id { get; set; }

        public int isActive { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string EmailAdd { get; set; }

        [Required]
        public string AccessType { get; set; }

        [Required]
        public string BUname { get; set; }

        [Display(Name = "Added By")]
        public string? addedBy { get; set; }

        public string? EditByUserName { get; set; }

        public string? DeleteByUserName { get; set; }

        [Display(Name = "is Approver")]
        public bool? isApprover { get; set; }

        //[Display(Name = "Approver Email Add")]
        //public string? ApproverEmailAdd { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Created Date")]
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

