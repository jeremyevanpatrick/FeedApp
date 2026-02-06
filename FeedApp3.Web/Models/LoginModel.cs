using System.ComponentModel.DataAnnotations;

namespace FeedApp3.Web.Models
{
    public class LoginModel
    {
        [Display(Name = "Email")]
        [Required]
        [EmailAddress]
        public string? LoginEmail { get; set; }

        [Display(Name = "Password")]
        [Required]
        [DataType(DataType.Password)]
        public string? LoginPassword { get; set; }
    }
}
