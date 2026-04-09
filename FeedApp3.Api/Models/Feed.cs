namespace FeedApp3.Api.Models
{
    public class Feed
    {
        public Guid FeedId { get; set; }

        public Guid UserId { get; set; }

        public string FeedUrl { get; set; }

        public string FeedTitle { get; set; }

        public DateTime LastChecked { get; set; }

        public DateTime? LastModified { get; set; }

        public string? FaviconImage { get; set; }

        public string BlogUrl { get; set; }

        // Unmapped
        public int UnreadArticleCount { get; set; }

        // Navigation property
        public List<Article> Articles { get; set; } = new List<Article>();

    }
}