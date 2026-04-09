using FeedApp3.Shared.Services.DTOs;

namespace FeedApp3.Web.Data
{
    public interface IFeedClient
    {
        Task<List<FeedDto>> GetFeedList();

        Task<FeedDto> GetFeedById(Guid feedId);

        Task Create(string feedUrl);

        Task Update(Guid feedId);

        Task Delete(Guid feedId);

        Task MarkFeedAsRead(Guid feedId);

        Task MarkArticleAsRead(Guid articleId);
    }
}
