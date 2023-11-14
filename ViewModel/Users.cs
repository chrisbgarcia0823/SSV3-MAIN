using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace ShopSuppliesV3.ViewModel
{
    [NotMapped]
    public class Users
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string EmailAdd { get; set; }

        [Required]
        public string AccessType { get; set; } //Admin, User, Buyer

        [Required]
        public string BUname { get; set; }

        [Display(Name = "Added By")]
        public string? addedBy { get; set; }

        [Display(Name = "Approver")]
        public bool? isApprover { get; set; } //Approver can also be a user but not buyer

        //[Required]
        //[Display(Name = "Approver Email Add")]
        //public string? ApproverEmailAdd { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Created Date")]
        public DateTime DateAdded { get; set; }
    }
}
