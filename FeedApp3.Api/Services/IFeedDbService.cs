using FeedApp3.Api.Models;

namespace FeedApp3.Api.Services
{
    public interface IFeedDbService
    {
        public Task<List<Feed>> GetListByUserId(Guid userId);

        public Task<Feed?> GetByFeedId(Guid userId, Guid feedId);

        public Task<Article?> GetByArticleId(Guid articleId);

        public Task<bool> IsDuplicateFeedUrl(Guid userId, string feedUrl);

        public Task CreateAsync(Feed newFeed);

        public Task UpdateAsync(Feed existingFeed, Feed latestFeedUntracked);

        public Task DeleteAsync(Feed feed);

        public Task MarkFeedAsReadAsync(Feed feed);

        public Task MarkArticleAsReadAsync(Article article);

        public Task DeleteUserDataAsync(Guid userId);

    }
}
