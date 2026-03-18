using FeedApp3.Shared.Services.DTOs;

namespace FeedApp3.Api.Services.External
{
    public interface IRssClient
    {
        Task<FeedDto> ImportFeedFromUrl(string feedUrl, DateTime? previousLastModified = null);
    }
}