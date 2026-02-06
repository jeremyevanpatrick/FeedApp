using System.ComponentModel.DataAnnotations;

namespace FeedApp3.Web.Models
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
    }
}
