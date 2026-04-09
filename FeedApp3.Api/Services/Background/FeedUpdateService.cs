using FeedApp3.Api.Data.Repositories;
using FeedApp3.Api.Helpers;
using FeedApp3.Api.Models;
using FeedApp3.Api.Services.External;
using FeedApp3.Api.Settings;
using FeedApp3.Shared.Errors;
using FeedApp3.Shared.Extensions;
using FeedApp3.Shared.Services.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FeedApp3.Api.Services.Background
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

                var feedRepository = scope.ServiceProvider.GetRequiredService<IFeedRepository>();

                List<FeedUpdate> feedUpdates = await feedRepository.GetPendingFeedUpdatesAsync(_applicationSettings.FeedUpdateBatchSize);
                if (!feedUpdates.Any())
                {
                    return;
                }

                using (_logger.BeginLoggingScope(nameof(FeedUpdateService), nameof(ProcessFeedUpdates), Guid.NewGuid().ToString()))
                {
                    await Parallel.ForEachAsync(feedUpdates,
                        new ParallelOptions
                        {
                            MaxDegreeOfParallelism = _applicationSettings.FeedUpdateMaxParallelism
                        },
                        async (feedUpdate, ct) =>
                        {
                            await ProcessSingleUserFeeds(feedUpdate);
                        });

                    await feedRepository.DeleteFeedUpdatesAsync(feedUpdates.Select(f => f.UserId).ToList());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while processing feed updates. ErrorCode: {ErrorCode}",
                    ApiErrorCodes.INTERNAL_SERVER_ERROR);
            }
        }

        private async Task ProcessSingleUserFeeds(FeedUpdate feedUpdate)
        {
            using (_logger.BeginLoggingScope(nameof(FeedUpdateService), nameof(ProcessSingleUserFeeds)))
            {
                Guid userId = Guid.Empty;
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var feedRepository = scope.ServiceProvider.GetRequiredService<IFeedRepository>();

                    userId = feedUpdate.UserId;

                    var feedList = await feedRepository.GetListByUserIdAsync(userId);

                    await UpdateFeedList(scope, feedList);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while processing feed updates. ErrorCode: {ErrorCode}, UserId: {UserId}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR,
                        userId.ToString());
                }
            }
        }

        private async Task UpdateFeedList(IServiceScope scope, List<Feed> feedSummaryList)
        {
            using (_logger.BeginLoggingScope(nameof(FeedUpdateService), nameof(UpdateFeedList)))
            {
                var feedRepository = scope.ServiceProvider.GetRequiredService<IFeedRepository>();

                var rssClient = scope.ServiceProvider.GetRequiredService<IRssClient>();

                foreach (Feed feedSummary in feedSummaryList)
                {
                    try
                    {
                        if (feedSummary.LastChecked < DateTime.UtcNow.AddMinutes(-_applicationSettings.MinimumActiveUserRefreshIntervalInMinutes))
                        {
                            Feed? existingFeed = await feedRepository.GetByFeedIdAsync(feedSummary.UserId, feedSummary.FeedId);
                            if (existingFeed == null)
                            {
                                _logger.LogError(
                                    null,
                                    "Existing feed not found during update. ErrorCode: {ErrorCode}, UserId: {UserId}, FeedId: {FeedId}",
                                    ApiErrorCodes.INVALID_REQUEST_PARAMETERS,
                                    feedSummary.UserId.ToString(),
                                    feedSummary.FeedId.ToString());
                            }
                            else
                            {
                                //Get latest feed from URL
                                FeedDto latestFeedDto = await rssClient.ImportFeedFromUrl(existingFeed.FeedUrl, existingFeed.LastModified);
                                Feed latestFeed = FeedMapper.ToEntity(latestFeedDto);

                                if (latestFeed.Articles.Any())
                                {
                                    List<Article> articlesToAdd = latestFeed.Articles.Where(l => !existingFeed.Articles.Any(e => e.ArticleUrl == l.ArticleUrl)).ToList();

                                    articlesToAdd.ForEach(a => a.FeedId = feedSummary.FeedId);

                                    await feedRepository.CreateArticlesAsync(articlesToAdd);

                                    existingFeed.LastModified = latestFeed.LastModified;
                                }
                                existingFeed.LastChecked = latestFeed.LastChecked;

                                await feedRepository.UpdateAsync(existingFeed);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Unexpected error while updating feed. ErrorCode: {ErrorCode}, UserId: {UserId}, FeedId: {FeedId}",
                            ApiErrorCodes.INTERNAL_SERVER_ERROR,
                            feedSummary.UserId.ToString(),
                            feedSummary.FeedId.ToString());
                    }
                }
            }
        }

    }
}