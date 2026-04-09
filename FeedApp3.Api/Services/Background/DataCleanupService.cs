using FeedApp3.Api.Data.Repositories;
using FeedApp3.Api.Models;
using FeedApp3.Api.Settings;
using FeedApp3.Shared.Errors;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using FeedApp3.Shared.Extensions;

namespace FeedApp3.Api.Services.Background
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
                using (_logger.BeginLoggingScope(nameof(DataCleanupService), nameof(ExecuteAsync), Guid.NewGuid().ToString()))
                {
                    await CleanupRefreshTokens();
                    await CleanupSoftDeletedUsers();
                    await CleanupApplicationLogs();
                }

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
            using (_logger.BeginLoggingScope(nameof(DataCleanupService), nameof(CleanupRefreshTokens)))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var authRepository = scope.ServiceProvider.GetRequiredService<IAuthRepository>();
                    await authRepository.DeleteRefreshTokensExpiredByDaysAsync(_dataCleanupSettings.PurgeTokensAfterDays);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while cleaning up refresh tokens. ErrorCode: {ErrorCode}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR);
                }
            }
        }

        private async Task CleanupSoftDeletedUsers()
        {
            using (_logger.BeginLoggingScope(nameof(DataCleanupService), nameof(CleanupSoftDeletedUsers)))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                    var feedRepository = scope.ServiceProvider.GetRequiredService<IFeedRepository>();

                    var authRepository = scope.ServiceProvider.GetRequiredService<IAuthRepository>();

                    var softDeletedUsers = await authRepository.GetSoftDeletedUsersAsync(_dataCleanupSettings.PurgeSoftDeletedAfterDays);

                    foreach (var user in softDeletedUsers)
                    {
                        try
                        {
                            var result = await userManager.DeleteAsync(user);
                            if (!result.Succeeded)
                            {
                                _logger.LogError(
                                    "Cleanup soft deleted users failed unexpectedly. ErrorCode: {ErrorCode}, UserId: {UserId}, Description: {Description}",
                                    ApiErrorCodes.INTERNAL_SERVER_ERROR,
                                    user.Id,
                                    string.Join(", ", result.Errors.Select(e => e.Description)));
                                break;
                            }

                            Guid userId = Guid.Parse(user.Id);
                            await feedRepository.DeleteUserFeedsAsync(userId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(
                                ex,
                                "Unexpected error while cleaning up soft deleted users. ErrorCode: {ErrorCode}, UserId: {UserId}",
                                ApiErrorCodes.INTERNAL_SERVER_ERROR,
                                user.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while cleaning up soft deleted users. ErrorCode: {ErrorCode}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR);
                }
            }
        }

        private async Task CleanupApplicationLogs()
        {
            using (_logger.BeginLoggingScope(nameof(DataCleanupService), nameof(CleanupApplicationLogs)))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var loggingDbService = scope.ServiceProvider.GetRequiredService<ILoggingRepository>();

                    await loggingDbService.DeleteInfoLogsByDaysAsync(_dataCleanupSettings.PurgeInfoLogsAfterDays);
                    await loggingDbService.DeleteErrorLogsByDaysAsync(_dataCleanupSettings.PurgeErrorLogsAfterDays);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while cleaning up application logs. ErrorCode: {ErrorCode}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR);
                }
            }
        }
    }
}