using FeedApp3.Shared.Services.DTOs;

namespace FeedApp3.Api.Services.Application
{
    public interface IFeedAppService
    {
        Task CreateAsync(Guid userId, string feedUrl);
        Task DeleteAsync(Guid userId, Guid feedId);
        Task<FeedDto> GetFeedByIdAsync(Guid userId, Guid feedId);
        Task<List<FeedDto>> GetFeedListAsync(Guid userId);
        Task MarkArticleAsReadAsync(Guid userId, Guid articleId);
        Task MarkFeedAsReadAsync(Guid userId, Guid feedId);
        Task UpdateAsync(Guid userId, Guid feedId);
    }
}