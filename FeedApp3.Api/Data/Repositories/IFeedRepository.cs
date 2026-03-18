using FeedApp3.Api.Models;

namespace FeedApp3.Api.Data.Repositories
{
    public interface IFeedRepository
    {
        public Task<List<Feed>> GetListByUserIdAsync(Guid userId);

        public Task<Feed?> GetByFeedIdAsync(Guid userId, Guid feedId);

        public Task<Article?> GetByArticleIdAsync(Guid userId, Guid articleId);

        public Task<bool> IsDuplicateFeedUrlAsync(Guid userId, string feedUrl);

        public Task CreateAsync(Feed feed);

        public Task UpdateAsync(Feed feed);

        public Task DeleteAsync(Feed feed);

        public Task CreateArticlesAsync(List<Article> articles);

        public Task UpdateArticleAsync(Article article);

        public Task DeleteUserFeedsAsync(Guid userId);

        public Task CreateFeedUpdateAsync(FeedUpdate feedUpdate);

        public Task<List<FeedUpdate>> GetPendingFeedUpdatesAsync(int batchSize);

        public Task DeleteFeedUpdatesAsync(List<Guid> userIds);

    }
}
