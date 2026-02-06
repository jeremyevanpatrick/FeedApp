using FeedApp3.Api.Models;
using FeedApp3.Shared.Services.DTOs;

namespace FeedApp3.Api.Helpers
{
    public static class FeedMapper
    {
        public static FeedDto? ToDto(Feed? f)
        {
            if (f == null)
            {
                return null;
            }

            return new FeedDto()
            {
                FeedId = f.FeedId,
                UserId = f.UserId,
                FeedTitle = f.FeedTitle,
                FeedUrl = f.FeedUrl,
                BlogUrl = f.BlogUrl,
                FaviconImage = f.FaviconImage,
                LastChecked = f.LastChecked,
                LastModified = f.LastModified,
                UnreadArticleCount = f.UnreadArticleCount,
                Articles = f.Articles.Select(a => new ArticleDto()
                {
                    ArticleId = a.ArticleId,
                    FeedId = a.FeedId,
                    ArticleTitle = a.ArticleTitle,
                    ArticleUrl = a.ArticleUrl,
                    ArticleDate = a.ArticleDate,
                    IsUnread = a.IsUnread,
                    ArticleContent = a.ArticleContent
                    
                }).ToList()
            };
        }

        public static Feed? ToEntity(FeedDto f)
        {
            if (f == null)
            {
                return null;
            }

            return new Feed()
            {
                FeedId = f.FeedId,
                UserId = f.UserId,
                FeedTitle = f.FeedTitle,
                FeedUrl = f.FeedUrl,
                BlogUrl = f.BlogUrl,
                FaviconImage = f.FaviconImage,
                LastChecked = f.LastChecked,
                LastModified = f.LastModified,
                UnreadArticleCount = f.UnreadArticleCount,
                Articles = f.Articles.Select(a => new Article()
                {
                    ArticleId = a.ArticleId,
                    FeedId = a.FeedId,
                    ArticleTitle = a.ArticleTitle,
                    ArticleUrl = a.ArticleUrl,
                    ArticleDate = a.ArticleDate,
                    IsUnread = a.IsUnread,
                    ArticleContent = a.ArticleContent

                }).ToList()
            };
        }
    }
}
