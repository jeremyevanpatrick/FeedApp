using FeedApp3.Api.Helpers;
using FeedApp3.Api.Models;
using FeedApp3.Api.Settings;
using FeedApp3.Shared.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FeedApp3.Api.Services
{
    public class DataCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DataCleanupService> _logger;
        private readonly DataCleanupSettings _dataCleanupSettings;

        public DataCleanupService(IServiceScopeFactory scopeFactory, ILogger<DataCleanupService> logger, IOptions<DataCleanupSettings> dataCleanupSettings)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _dataCleanupSettings = dataCleanupSettings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await CleanupRefreshTokens();
                await CleanupSoftDeletedUsers();

                var delay = GetScheduledDelay(_dataCleanupSettings.ScheduledHour);
                await Task.Delay(delay, stoppingToken);
            }
        }

        private TimeSpan GetScheduledDelay(int scheduledHour)
        {
            var now = DateTime.UtcNow;
            var nextRun = new DateTime(now.Year, now.Month, now.Day, scheduledHour, 0, 0, DateTimeKind.Utc);

            if (now > nextRun)
            {
                nextRun = nextRun.AddDays(1);
            }

            return nextRun - now;
        }

        private async Task CleanupRefreshTokens()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var authDbService = scope.ServiceProvider.GetRequiredService<IAuthDbService>();
                await authDbService.DeleteExpiredRefreshTokensAsync(_dataCleanupSettings.OlderThanDays);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(CleanupErrorCodes.CleanupRefreshTokensUnexpected, ex, "Unexpected error while cleaning up refresh tokens", new Dictionary<string, string> { });
            }
        }

        private async Task CleanupSoftDeletedUsers()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                var feedDbService = scope.ServiceProvider.GetRequiredService<IFeedDbService>();

                var authDbService = scope.ServiceProvider.GetRequiredService<IAuthDbService>();

                var softDeletedUsers = await authDbService.GetSoftDeletedUsers(_dataCleanupSettings.OlderThanDays);

                foreach (var user in softDeletedUsers)
                {
                    try
                    {
                        var result = await userManager.DeleteAsync(user);
                        if (!result.Succeeded)
                        {
                            _logger.LogErrorWithDictionary(CleanupErrorCodes.CleanupSoftDeletedUsersFailed, null, "Cleanup soft deleted users failed unexpectedly", new Dictionary<string, string> {
                                { "UserId", user.Id },
                                { "Description", string.Join(", ", result.Errors.Select(e => e.Description)) }
                            });
                        }

                        Guid userId = Guid.Parse(user.Id);
                        await feedDbService.DeleteUserDataAsync(userId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogErrorWithDictionary(CleanupErrorCodes.CleanupSoftDeletedUsersUnexpected, ex, "Unexpected error while cleaning up soft deleted users", new Dictionary<string, string> {
                            { "UserId", user.Id }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(CleanupErrorCodes.CleanupSoftDeletedUsersUnexpected, ex, "Unexpected error while cleaning up soft deleted users", new Dictionary<string, string> { });
            }
        }
    }
}