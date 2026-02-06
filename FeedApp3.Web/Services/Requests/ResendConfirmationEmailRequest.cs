using System.ComponentModel.DataAnnotations;

namespace FeedApp3.Web.Services.Requests
{
    public class ResendConfirmationEmailRequest
    {
        public ResendConfirmationEmailRequest(string email)
        {
            Email = email;
        }

        [Required]
        public string Email { get; set; }
    }
}
