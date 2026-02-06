using System.ComponentModel.DataAnnotations;

namespace FeedApp3.Web.Models
{
    public class ChangeEmailModel
    {
        [Required]
        [EmailAddress]
        public string? NewEmail { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}
