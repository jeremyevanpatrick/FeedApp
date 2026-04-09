using FeedApp3.Api.Data.Context;
using FeedApp3.Api.Models;
using Microsoft.EntityFrameworkCore;
using FeedApp3.Shared.Extensions;

namespace FeedApp3.Api.Data.Repositories
{
    public class FeedRepository : IFeedRepository
    {
        private readonly ILogger<FeedRepository> _logger;
        private readonly AppDbContext _db;

        public FeedRepository(
            ILogger<FeedRepository> logger,
            AppDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<List<Feed>> GetListByUserIdAsync(Guid userId)
        {
            using (_logger.BeginLoggingScope(nameof(FeedRepository), nameof(GetListByUserIdAsync)))
            {
                return await _db.Feeds
                    .Include(f => f.Articles)
                    .Where(f => f.UserId == userId)
                    .AsNoTracking()
                    .ToListAsync();
            }
        }

        public async Task<Feed?> GetByFeedIdAsync(Guid userId, Guid feedId)
        {
            using (_logger.BeginLoggingScope(nameof(FeedRepository), nameof(GetByFeedIdAsync)))
            {
                return await _db.Feeds
                    .Include(f => f.Articles)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(f =>
                        f.UserId == userId &&
                        f.FeedId == feedId);
            }
        }

        public async Task<Article?> GetByArticleIdAsync(Guid userId, Guid articleId)
        {
            using (_logger.BeginLoggingScope(nameof(FeedRepository), nameof(GetByArticleIdAsync)))
            {
                return await _db.Articles
                    .Include(a => a.Feed)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.ArticleId == articleId && a.Feed.UserId == userId);
            }
        }

        public async Task<bool> IsDuplicateFeedUrlAsync(Guid userId, string feedUrl)
        {
            using (_logger.BeginLoggingScope(nameof(FeedRepository), nameof(IsDuplicateFeedUrlAsync)))
            {
                return await _db.Feeds.AnyAsync(f => f.UserId == userId && f.FeedUrl == feedUrl);
            }
        }

        public async Task CreateAsync(Feed feed)
        {
            using (_logger.BeginLoggingScope(nameof(FeedRepository), nameof(CreateAsync)))
            {
                _db.Feeds.Add(feed);
                await _db.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(Feed feed)
        {
            using (_logger.BeginLoggingScope(nameof(FeedRepository), nameof(UpdateAsync)))
            {
                _db.Feeds.Update(feed);
                await _db.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(Feed feed)
        {
            using (_logger.BeginLoggingScope(nameof(FeedRepository), nameof(DeleteAsync)))
            {
                _db.Feeds.Remove(feed);
                await _db.SaveChangesAsync();
            }
        }

        public async Task CreateArticlesAsync(List<Article> articles)
        {
            using (_logger.BeginLoggingScope(nameof(FeedRepository), nameof(CreateArticlesAsync)))
            {
                _db.Articles.AddRange(articles);
                await _db.SaveChangesAsync();
            }
        }

        public async Task UpdateArticleAsync(Article article)
        {
            using (_logger.BeginLoggingScope(nameof(FeedRepository), nameof(UpdateArticleAsync)))
            {
                _db.Articles.Update(article);
                await _db.SaveChangesAsync();
            }
        }

        public async Task DeleteUserFeedsAsync(Guid userId)
        {
            using (_logger.BeginLoggingScope(nameof(FeedRepository), nameof(DeleteUserFeedsAsync)))
            {
                await _db.Feeds
                .Where(f => f.UserId == userId)
                .ExecuteDeleteAsync();
            }
        }

        public async Task CreateFeedUpdateAsync(FeedUpdate feedUpdate)
        {
            using (_logger.BeginLoggingScope(nameof(FeedRepository), nameof(CreateFeedUpdateAsync)))
            {
                _db.FeedUpdates.Add(feedUpdate);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<List<FeedUpdate>> GetPendingFeedUpdatesAsync(int batchSize)
        {
            using (_logger.BeginLoggingScope(nameof(FeedRepository), nameof(GetPendingFeedUpdatesAsync)))
            {
                return (await _db.FeedUpdates
                        .OrderBy(f => f.RequestedAt)
                        .Take(batchSize)
                        .AsNoTracking()
                        .ToListAsync())
                    .DistinctBy(f => f.UserId)
                    .ToList();
            }
        }

        public async Task DeleteFeedUpdatesAsync(List<Guid> userIds)
        {
            using (_logger.BeginLoggingScope(nameof(FeedRepository), nameof(DeleteFeedUpdatesAsync)))
            {
                await _db.FeedUpdates
                    .Where(u => userIds.Contains(u.UserId))
                    .ExecuteDeleteAsync();
            }
        }

    }
}
