using FeedApp3.Api.Data.Context;
using FeedApp3.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FeedApp3.Api.Data.Repositories
{
    public class FeedRepository : IFeedRepository
    {
        private readonly AppDbContext _db;

        public FeedRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<Feed>> GetListByUserIdAsync(Guid userId)
        {
            return await _db.Feeds
                .Include(f => f.Articles)
                .Where(f => f.UserId == userId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Feed?> GetByFeedIdAsync(Guid userId, Guid feedId)
        {
            return await _db.Feeds
                .Include(f => f.Articles)
                .AsNoTracking()
                .FirstOrDefaultAsync(f =>
                    f.UserId == userId &&
                    f.FeedId == feedId);
        }

        public async Task<Article?> GetByArticleIdAsync(Guid userId, Guid articleId)
        {
            //TODO: verify if this works
            return await _db.Articles
                .Include(a => a.Feed)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.ArticleId == articleId && a.Feed.UserId == userId);
        }

        public async Task<bool> IsDuplicateFeedUrlAsync(Guid userId, string feedUrl)
        {
            return await _db.Feeds.AnyAsync(f => f.UserId == userId && f.FeedUrl == feedUrl);
        }

        public async Task CreateAsync(Feed feed)
        {
            _db.Feeds.Add(feed);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Feed feed)
        {
            _db.Feeds.Update(feed);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Feed feed)
        {
            _db.Feeds.Remove(feed);
            await _db.SaveChangesAsync();
        }

        public async Task CreateArticlesAsync(List<Article> articles)
        {
            _db.Articles.AddRange(articles);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateArticleAsync(Article article)
        {
            _db.Articles.Update(article);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteUserFeedsAsync(Guid userId)
        {
            await _db.Feeds
                .Where(f => f.UserId == userId)
                .ExecuteDeleteAsync();
        }

        public async Task CreateFeedUpdateAsync(FeedUpdate feedUpdate)
        {
            _db.FeedUpdates.Add(feedUpdate);
            await _db.SaveChangesAsync();
        }

        public async Task<List<FeedUpdate>> GetPendingFeedUpdatesAsync(int batchSize)
        {
            //TODO: verify if this works
            return await _db.FeedUpdates
                .GroupBy(f => f.UserId)
                .Select(g => g.OrderBy(f => f.RequestedAt).First())
                .Take(batchSize)
                .AsNoTracking()
                .ToListAsync();

            /*var result = await _db.FeedUpdates
                .OrderBy(f => f.RequestedAt)
                .AsNoTracking()
                .ToListAsync();

            return result
                .GroupBy(f => f.UserId)
                .Select(g => g.First())
                .Take(batchSize)
                .ToList();*/
        }

        public async Task DeleteFeedUpdatesAsync(List<Guid> userIds)
        {
            await _db.FeedUpdates
                .Where(u => userIds.Contains(u.UserId))
                .ExecuteDeleteAsync();
        }

    }
}
