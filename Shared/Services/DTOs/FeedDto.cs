using System.ComponentModel.DataAnnotations;

namespace FeedApp3.Shared.Services.DTOs
{
    public class FeedDto
    {
        [Required]
        public Guid FeedId { get; set; }
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public string FeedUrl { get; set; }
        [Required]
        public string FeedTitle { get; set; }
        public string? FaviconImage { get; set; }
        [Required]
        public string BlogUrl { get; set; }
        [Required]
        public DateTime LastChecked { get; set; }
        public DateTime? LastModified { get; set; }
        public List<ArticleDto> Articles { get; set; } = new List<ArticleDto>();

        public int UnreadArticleCount { get; set; }
    }
}