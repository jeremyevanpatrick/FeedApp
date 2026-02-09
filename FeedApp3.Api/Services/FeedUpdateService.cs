using FeedApp3.Api.Helpers;
using FeedApp3.Api.Models;
using FeedApp3.Api.Settings;
using FeedApp3.Shared.Helpers;
using FeedApp3.Shared.Services.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FeedApp3.Api.Services
{
    public class FeedUpdateService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<FeedUpdateService> _logger;
        private readonly ApplicationSettings _applicationSettings;

        public FeedUpdateService(
            IServiceScopeFactory scopeFactory,
            ILogger<FeedUpdateService> logger,
            IOptions<ApplicationSettings> applicationSettings)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _applicationSettings = applicationSettings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessFeedUpdates();

                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task ProcessFeedUpdates()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var feedDbService = scope.ServiceProvider.GetRequiredService<IFeedDbService>();

                List<FeedUpdate> feedUpdates = await feedDbService.GetPendingFeedUpdatesAsync(_applicationSettings.FeedUpdateBatchSize);
                if (!feedUpdates.Any())
                {
                    return;
                }

                await Parallel.ForEachAsync(feedUpdates,
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = _applicationSettings.FeedUpdateMaxParallelism
                    },
                    async (feedUpdate, ct) =>
                    {
                        await ProcessSingleUserFeeds(feedUpdate);
                    });

                await feedDbService.DeleteFeedUpdatesAsync(feedUpdates.Select(f => f.UserId).ToList());

            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(FeedUpdateErrorCodes.ProcessFeedUpdatesUnexpected, ex, "Unexpected error while processing feed updates");
            }
        }

        private async Task ProcessSingleUserFeeds(FeedUpdate feedUpdate)
        {
            Guid userId = Guid.Empty;
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var feedDbService = scope.ServiceProvider.GetRequiredService<IFeedDbService>();

                userId = feedUpdate.UserId;

                var feedList = await feedDbService.GetListByUserId(userId);

                await UpdateFeedList(scope, feedList);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(FeedUpdateErrorCodes.ProcessSingleUserFeedsUnexpected, ex, "Unexpected error while processing feed updates", new Dictionary<string, string> {
                    { "UserId", userId.ToString() }
                });
            }
        }

        private async Task UpdateFeedList(IServiceScope scope, List<Feed> feedSummaryList)
        {
            var feedDbService = scope.ServiceProvider.GetRequiredService<IFeedDbService>();

            var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

            var rssClient = new RssHttpClient(httpClientFactory);

            foreach (Feed feedSummary in feedSummaryList)
            {
                try
                {
                    if (feedSummary.LastChecked < DateTime.UtcNow.AddMinutes(-_applicationSettings.MinimumActiveUserRefreshIntervalInMinutes))
                    {
                        Feed? existingFeed = await feedDbService.GetByFeedId(feedSummary.UserId, feedSummary.FeedId);
                        if (existingFeed == null)
                        {
                            _logger.LogErrorWithDictionary(FeedUpdateErrorCodes.UpdateFeedNotFound, null, "Existing feed not found during update", new Dictionary<string, string> {
                                { "UserId", feedSummary.UserId.ToString() },
                                { "FeedId", feedSummary.FeedId.ToString() }
                            });
                        }
                        else
                        {
                            FeedDto latestFeedDto = await rssClient.ImportFeedFromUrl(existingFeed.FeedUrl, existingFeed.LastModified);
                            Feed latestFeed = FeedMapper.ToEntity(latestFeedDto);
                            await feedDbService.UpdateAsync(existingFeed, latestFeed);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogErrorWithDictionary(FeedUpdateErrorCodes.UpdateFeedInBackgroundUnexpected, ex, "Unexpected error while updating feed", new Dictionary<string, string> {
                        { "UserId", feedSummary.UserId.ToString() },
                        { "FeedId", feedSummary.FeedId.ToString() }
                    });
                }
            }
        }

    }
}