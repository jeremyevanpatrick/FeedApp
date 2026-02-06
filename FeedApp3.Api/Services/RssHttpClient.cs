using CodeHollow.FeedReader;
using FeedApp3.Shared.Services.DTOs;

namespace FeedApp3.Api.Services
{
    public class RssHttpClient : IRssHttpClient
    {
        private readonly HttpClient _httpClient;

        public RssHttpClient(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("PublicApi");
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<FeedDto> ImportFeedFromUrl(string feedUrl, DateTime? previousLastModified = null)
        {
            DateTime lastChecked = DateTime.UtcNow;

            Feed codeHollowFeed = await FeedReader.ReadAsync(feedUrl);

            FeedDto feedDto = MapMapFeedReaderFeedToDto(codeHollowFeed, previousLastModified ?? DateTime.UtcNow.AddYears(-5));
            feedDto.FeedUrl = feedUrl;
            feedDto.LastChecked = lastChecked;
            if (feedDto.Articles.Any())
            {
                feedDto.LastModified = feedDto.Articles.Max(a => a.ArticleDate);
            }

            return feedDto;
        }

        private FeedDto MapMapFeedReaderFeedToDto(Feed codeHollowFeed, DateTime previousLastModified)
        {
            FeedDto feedDto = new FeedDto()
            {
                FeedTitle = codeHollowFeed.Title,
                BlogUrl = codeHollowFeed.Link,
                FaviconImage = codeHollowFeed.ImageUrl,
                Articles = codeHollowFeed.Items
                    .Where(i => !i.PublishingDate.HasValue || i.PublishingDate > previousLastModified)
                    .Select(i => MapFeedReaderArticleToDto(i)).ToList()
            };

            return feedDto;
        }

        private ArticleDto MapFeedReaderArticleToDto(FeedItem codeHollowArticle)
        {
            return new ArticleDto()
            {
                ArticleTitle = codeHollowArticle.Title,
                ArticleUrl = codeHollowArticle.Link,
                ArticleDate = codeHollowArticle.PublishingDate ?? DateTime.UtcNow,
                ArticleContent = codeHollowArticle.Content ?? codeHollowArticle.Description ?? string.Empty,
                IsUnread = true
            };
        }
    }
}
