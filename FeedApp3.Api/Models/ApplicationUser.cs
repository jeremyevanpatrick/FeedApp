using Microsoft.AspNetCore.Identity;

namespace FeedApp3.Api.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
