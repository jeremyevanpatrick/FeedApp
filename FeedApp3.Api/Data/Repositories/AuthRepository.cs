using FeedApp3.Api.Data.Context;
using FeedApp3.Api.Models;
using Microsoft.EntityFrameworkCore;
using FeedApp3.Shared.Extensions;

namespace FeedApp3.Api.Data.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ILogger<AuthRepository> _logger;
        private readonly AuthDbContext _db;

        public AuthRepository(
            ILogger<AuthRepository> logger,
            AuthDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<List<ApplicationUser>> GetSoftDeletedUsersAsync(int olderThanDays = 0)
        {
            using (_logger.BeginLoggingScope(nameof(AuthRepository), nameof(GetSoftDeletedUsersAsync)))
            {
                return await _db.Users
                    .IgnoreQueryFilters()
                    .Where(u => u.IsDeleted && u.DeletedAt < DateTime.UtcNow.AddDays(-olderThanDays))
                    .AsNoTracking()
                    .ToListAsync();
            }
        }

        public async Task<List<RefreshToken>> GetUserRefreshTokensAsync(string userId)
        {
            using (_logger.BeginLoggingScope(nameof(AuthRepository), nameof(GetUserRefreshTokensAsync)))
            {
                return await _db.RefreshTokens
                    .Where(t => t.UserId == userId && !t.IsRevoked)
                    .AsNoTracking()
                    .ToListAsync();
            }
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string refreshTokenHash)
        {
            using (_logger.BeginLoggingScope(nameof(AuthRepository), nameof(GetRefreshTokenAsync)))
            {
                return await _db.RefreshTokens
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.TokenHash == refreshTokenHash);
            }
        }

        public async Task AddRefreshTokenAsync(RefreshToken refreshToken)
        {
            using (_logger.BeginLoggingScope(nameof(AuthRepository), nameof(AddRefreshTokenAsync)))
            {
                _db.RefreshTokens.Add(refreshToken);
                await _db.SaveChangesAsync();
            }
        }

        public async Task UpdateRefreshTokenAsync(RefreshToken refreshToken)
        {
            using (_logger.BeginLoggingScope(nameof(AuthRepository), nameof(UpdateRefreshTokenAsync)))
            {
                _db.RefreshTokens.Update(refreshToken);
                await _db.SaveChangesAsync();
            }
        }

        public async Task UpdateRefreshTokenRangeAsync(List<RefreshToken> refreshTokenList)
        {
            using (_logger.BeginLoggingScope(nameof(AuthRepository), nameof(UpdateRefreshTokenRangeAsync)))
            {
                _db.RefreshTokens.UpdateRange(refreshTokenList);
                await _db.SaveChangesAsync();
            }
        }

        public async Task DeleteRefreshTokensExpiredByDaysAsync(int olderThanDays = 0)
        {
            using (_logger.BeginLoggingScope(nameof(AuthRepository), nameof(DeleteRefreshTokensExpiredByDaysAsync)))
            {
                await _db.RefreshTokens
                    .Where(t => t.ExpiresAt < DateTime.UtcNow.AddDays(-olderThanDays))
                    .ExecuteDeleteAsync();
            }
        }
    }
}
