using FeedApp3.Shared.Services.DTOs;

namespace FeedApp3.Api.Services
{
    public interface IRssHttpClient
    {
        Task<FeedDto> ImportFeedFromUrl(string feedUrl, DateTime? previousLastModified);
    }
}