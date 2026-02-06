using FeedApp3.Api.Models;

namespace FeedApp3.Api.Services
{
    public interface IAuthDbService
    {
        Task<List<ApplicationUser>> GetSoftDeletedUsers(int olderThanDays = 0);
        Task<RefreshToken?> GetRefreshTokenAsync(string refreshTokenHash);
        Task AddRefreshTokenAsync(RefreshToken refreshToken);
        Task RevokeRefreshTokenAsync(string refreshTokenHash);
        Task RevokeAllUserRefreshTokensAsync(string userId);
        Task DeleteExpiredRefreshTokensAsync(int olderThanDays = 0);
    }
}
