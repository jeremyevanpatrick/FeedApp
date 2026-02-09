using FeedApp3.Api.Data;
using FeedApp3.Api.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace FeedApp3.Api.Services
{
    public class FeedDbService : IFeedDbService
    {
        private readonly AppDbContext _db;

        public FeedDbService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<Feed>> GetListByUserId(Guid userId)
        {
            return await _db.Feeds
                .Where(f => f.UserId == userId)
                .Select(f => new Feed
                {
                    FeedId = f.FeedId,
                    UserId = f.UserId,
                    BlogUrl = f.BlogUrl,
                    FaviconImage = f.FaviconImage,
                    FeedTitle = f.FeedTitle,
                    FeedUrl = f.FeedUrl,
                    LastChecked = f.LastChecked,
                    LastModified = f.LastModified,
                    UnreadArticleCount = f.Articles.Count(a => a.IsUnread)
                })
                .OrderByDescending(f => f.UnreadArticleCount)
                .ToListAsync();
        }

        public async Task<Feed?> GetByFeedId(Guid userId, Guid feedId)
        {
            var feed = await _db.Feeds
                .Include(f => f.Articles.OrderByDescending(a => a.ArticleDate))
                .FirstOrDefaultAsync(f =>
                    f.UserId == userId &&
                    f.FeedId == feedId);

            if (feed != null)
            {
                feed.UnreadArticleCount = feed.Articles?.Count(a => a.IsUnread) ?? 0;
            }

            return feed;
        }

        public async Task<Article?> GetByArticleId(Guid articleId)
        {
            return await _db.Articles
                .FirstOrDefaultAsync(a => a.ArticleId == articleId);
        }

        public async Task<bool> IsDuplicateFeedUrl(Guid userId, string feedUrl)
        {
            return await _db.Feeds.AnyAsync(f=> f.UserId == userId && f.FeedUrl == feedUrl);
        }

        public async Task CreateAsync(Feed newFeed)
        {
            _db.Feeds.Add(newFeed);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Feed existingFeed, Feed latestFeedUntracked)
        {
            if (latestFeedUntracked.Articles.Any())
            {
                foreach (Article newArticle in latestFeedUntracked.Articles)
                {
                    Article? existingArticle = existingFeed.Articles.FirstOrDefault(a => a.ArticleUrl == newArticle.ArticleUrl);
                    if (existingArticle == null)
                    {
                        existingFeed.Articles.Add(newArticle);
                        _db.Entry(newArticle).State = EntityState.Added;
                    }
                }
                existingFeed.LastModified = latestFeedUntracked.LastModified;
            }
            existingFeed.LastChecked = latestFeedUntracked.LastChecked;

            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Feed feed)
        {
            _db.Feeds.Remove(feed);
            await _db.SaveChangesAsync();
        }

        public async Task MarkFeedAsReadAsync(Feed feed)
        {
            foreach (Article article in feed.Articles)
            {
                article.IsUnread = false;
            }
            await _db.SaveChangesAsync();
        }

        public async Task MarkArticleAsReadAsync(Article article)
        {
            article.IsUnread = false;
            await _db.SaveChangesAsync();
        }

        public async Task DeleteUserDataAsync(Guid userId)
        {
            await _db.Feeds
                .Where(f => f.UserId == userId)
                .ExecuteDeleteAsync();
        }

        public async Task CreateFeedUpdateAsync(Guid userId)
        {
            var isAlreadyRequested = await _db.FeedUpdates.AnyAsync(u => u.UserId == userId);
            if (isAlreadyRequested)
            {
                return;
            }

            _db.FeedUpdates.Add(new FeedUpdate
            {
                UserId = userId,
                RequestedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }

        public async Task<List<FeedUpdate>> GetPendingFeedUpdatesAsync(int batchSize)
        {
            var result = await _db.FeedUpdates
                .OrderBy(f => f.RequestedAt)
                .AsNoTracking()
                .ToListAsync();

            return result
                .GroupBy(f => f.UserId)
                .Select(g => g.First())
                .Take(batchSize)
                .ToList();
        }

        public async Task DeleteFeedUpdatesAsync(List<Guid> userIds)
        {
            await _db.FeedUpdates
                .Where(u => userIds.Contains(u.UserId))
                .ExecuteDeleteAsync();
        }

    }
}
