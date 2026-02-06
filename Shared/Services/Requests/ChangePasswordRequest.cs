using System.ComponentModel.DataAnnotations;

namespace FeedApp3.Shared.Services.Requests
{
    public class ChangePasswordRequest
    {
        public ChangePasswordRequest(string existingPassword, string newPassword)
        {
            ExistingPassword = existingPassword;
            NewPassword = newPassword;
        }

        [Required]
        public string ExistingPassword { get; set; }

        [Required]
        public string NewPassword { get; set; }
    }
}
