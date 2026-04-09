using FeedApp3.Api.Data.Repositories;
using FeedApp3.Api.Exceptions;
using FeedApp3.Api.Helpers;
using FeedApp3.Api.Models;
using FeedApp3.Api.Services.External;
using FeedApp3.Shared.Services.DTOs;
using Microsoft.EntityFrameworkCore;
using FeedApp3.Shared.Extensions;

namespace FeedApp3.Api.Services.Application
{
    public class FeedAppService : IFeedAppService
    {
        private readonly ILogger<FeedAppService> _logger;
        private readonly IFeedRepository _feedRepository;
        private readonly IRssClient _rssClient;

        public FeedAppService(
            ILogger<FeedAppService> logger,
            IFeedRepository feedRepository,
            IRssClient rssClient)
        {
            _logger = logger;
            _feedRepository = feedRepository;
            _rssClient = rssClient;
        }

        public async Task<List<FeedDto>> GetFeedListAsync(Guid userId)
        {
            using (_logger.BeginLoggingScope(nameof(FeedAppService), nameof(GetFeedListAsync)))
            {
                List<Feed> feeds = await _feedRepository.GetListByUserIdAsync(userId);
                List<FeedDto> feedDtos = feeds.Select(f =>
                {
                    f.UnreadArticleCount = f.Articles.Count(a => a.IsUnread);
                    return FeedMapper.ToDto(f);
                })
                .OrderByDescending(f => f.UnreadArticleCount)
                .ToList();

                await _feedRepository.CreateFeedUpdateAsync(new FeedUpdate
                {
                    UserId = userId,
                    RequestedAt = DateTime.UtcNow
                });

                return feedDtos;
            }
        }

        public async Task<FeedDto> GetFeedByIdAsync(Guid userId, Guid feedId)
        {
            using (_logger.BeginLoggingScope(nameof(FeedAppService), nameof(GetFeedByIdAsync)))
            {
                Feed? feed = await _feedRepository.GetByFeedIdAsync(userId, feedId);
                if (feed == null)
                {
                    throw new NotFoundException("feedId");
                }

                feed.Articles = feed.Articles.OrderByDescending(a => a.ArticleDate).ToList();
                feed.UnreadArticleCount = feed.Articles.Count(f => f.IsUnread);

                return FeedMapper.ToDto(feed);
            }
        }

        public async Task CreateAsync(Guid userId, string feedUrl)
        {
            using (_logger.BeginLoggingScope(nameof(FeedAppService), nameof(CreateAsync)))
            {
                bool isDuplicateUrl = await _feedRepository.IsDuplicateFeedUrlAsync(userId, feedUrl);
                if (isDuplicateUrl)
                {
                    return;
                }

                FeedDto? feedDto = null;

                try
                {
                    feedDto = await _rssClient.ImportFeedFromUrl(feedUrl);
                }
                catch (Exception ex)
                {
                    throw new NotFoundException("feed url");
                }

                Feed feed = FeedMapper.ToEntity(feedDto);
                feed.UserId = userId;
                await _feedRepository.CreateAsync(feed);
            }
        }

        public async Task UpdateAsync(Guid userId, Guid feedId)
        {
            using (_logger.BeginLoggingScope(nameof(FeedAppService), nameof(UpdateAsync)))
            {
                Feed? existingFeed = await _feedRepository.GetByFeedIdAsync(userId, feedId);
                if (existingFeed == null)
                {
                    throw new NotFoundException("feedId");
                }

                //Get latest feed from URL
                FeedDto latestFeedDto = await _rssClient.ImportFeedFromUrl(existingFeed.FeedUrl, existingFeed.LastModified);
                Feed latestFeed = FeedMapper.ToEntity(latestFeedDto);

                if (latestFeed.Articles.Any())
                {
                    List<Article> articlesToAdd = latestFeed.Articles.Where(l => !existingFeed.Articles.Any(e => e.ArticleUrl == l.ArticleUrl)).ToList();
                    
                    articlesToAdd.ForEach(a => a.FeedId = feedId);
                    
                    await _feedRepository.CreateArticlesAsync(articlesToAdd);

                    existingFeed.LastModified = latestFeed.LastModified;
                }
                existingFeed.LastChecked = latestFeed.LastChecked;

                await _feedRepository.UpdateAsync(existingFeed);
            }
        }

        public async Task DeleteAsync(Guid userId, Guid feedId)
        {
            using (_logger.BeginLoggingScope(nameof(FeedAppService), nameof(DeleteAsync)))
            {
                Feed? existingFeed = await _feedRepository.GetByFeedIdAsync(userId, feedId);
                if (existingFeed == null)
                {
                    throw new NotFoundException("feedId");
                }

                await _feedRepository.DeleteAsync(existingFeed);
            }
        }

        public async Task MarkFeedAsReadAsync(Guid userId, Guid feedId)
        {
            using (_logger.BeginLoggingScope(nameof(FeedAppService), nameof(MarkFeedAsReadAsync)))
            {
                Feed? existingFeed = await _feedRepository.GetByFeedIdAsync(userId, feedId);
                if (existingFeed == null)
                {
                    throw new NotFoundException("feedId");
                }

                foreach (Article article in existingFeed.Articles)
                {
                    article.IsUnread = false;
                }

                await _feedRepository.UpdateAsync(existingFeed);
            }
        }

        public async Task MarkArticleAsReadAsync(Guid userId, Guid articleId)
        {
            using (_logger.BeginLoggingScope(nameof(FeedAppService), nameof(MarkArticleAsReadAsync)))
            {
                Article? existingArticle = await _feedRepository.GetByArticleIdAsync(userId, articleId);
                if (existingArticle == null)
                {
                    throw new NotFoundException("articleId");
                }

                existingArticle.IsUnread = false;

                await _feedRepository.UpdateArticleAsync(existingArticle);
            }
        }

    }
}
