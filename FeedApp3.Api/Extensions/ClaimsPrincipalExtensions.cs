using System.Security.Claims;

namespace FeedApp3.Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(value, out var id))
            {
                return id;
            }
            throw new UnauthorizedAccessException("Invalid user ID claim");
        }
    }
}
