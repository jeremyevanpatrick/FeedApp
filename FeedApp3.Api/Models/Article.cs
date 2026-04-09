namespace FeedApp3.Api.Models
{
    public class Article
    {
        public Guid ArticleId { get; set; }

        public Guid FeedId { get; set; }

        public string ArticleUrl { get; set; }

        public string ArticleTitle { get; set; }

        public string ArticleContent { get; set; }

        public bool IsUnread { get; set; }

        public DateTime ArticleDate { get; set; }

        // Navigation property
        public Feed Feed { get; set; } = null!;
    }
}