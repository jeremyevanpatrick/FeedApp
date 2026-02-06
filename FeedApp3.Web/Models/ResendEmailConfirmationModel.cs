using System.ComponentModel.DataAnnotations;

namespace FeedApp3.Web.Models
{
    public class ResendEmailConfirmationModel
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
    }
}
