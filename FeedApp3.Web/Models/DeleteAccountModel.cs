using System.ComponentModel.DataAnnotations;

namespace FeedApp3.Web.Models
{
    public class DeleteAccountModel
    {
        [Display(Name = "Password")]
        [Required]
        [DataType(DataType.Password)]
        public string? DeleteAccountPassword { get; set; }
    }
}
