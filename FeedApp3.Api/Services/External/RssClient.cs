using CodeHollow.FeedReader;
using FeedApp3.Shared.Services.DTOs;

namespace FeedApp3.Api.Services.External
{
    public class RssClient : IRssClient
    {
        public async Task<FeedDto> ImportFeedFromUrl(string feedUrl, DateTime? previousLastModified = null)
        {
            DateTime lastChecked = DateTime.UtcNow;

            Feed codeHollowFeed = await FeedReader.ReadAsync(feedUrl);

            FeedDto feedDto = new FeedDto()
            {
                FeedUrl = feedUrl,
                LastChecked = lastChecked,
                FeedTitle = codeHollowFeed.Title,
                BlogUrl = codeHollowFeed.Link,
                FaviconImage = codeHollowFeed.ImageUrl,
                Articles = codeHollowFeed.Items
                    .Where(i => !i.PublishingDate.HasValue || i.PublishingDate > (previousLastModified ?? DateTime.UtcNow.AddYears(-5)))
                    .Select(i => new ArticleDto()
                    {
                        ArticleTitle = i.Title,
                        ArticleUrl = i.Link,
                        ArticleDate = i.PublishingDate ?? DateTime.UtcNow,
                        ArticleContent = i.Content ?? i.Description ?? string.Empty,
                        IsUnread = true
                    }).ToList()
            };

            if (feedDto.Articles.Any())
            {
                feedDto.LastModified = feedDto.Articles.Max(a => a.ArticleDate);
            }

            return feedDto;
        }
    }
}
