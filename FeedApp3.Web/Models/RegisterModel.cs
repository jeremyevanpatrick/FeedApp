using System.ComponentModel.DataAnnotations;

namespace FeedApp3.Web.Models
{
    public class RegisterModel
    {
        [Display(Name = "Email")]
        [Required]
        [EmailAddress]
        public string? RegisterEmail { get; set; }

        [Display(Name = "Password")]
        [Required]
        [StringLength(
            100,
            MinimumLength = 8,
            ErrorMessage = "Password must be at least 8 characters long"
        )]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character"
        )]
        [DataType(DataType.Password)]
        public string? RegisterPassword { get; set; }

        [Display(Name = "Password Repeat")]
        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(RegisterPassword), ErrorMessage = "Passwords do not match.")]
        public string? RegisterPasswordRepeat { get; set; }
    }
}
