using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FeedApp3.Api.Models
{
    public class Feed
    {
        [Key]
        public Guid FeedId { get; set; }
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public string FeedUrl { get; set; }
        [Required]
        public string FeedTitle { get; set; }
        [Required]
        public DateTime LastChecked { get; set; }
        public DateTime? LastModified { get; set; }
        public string? FaviconImage { get; set; }
        [Required]
        public string BlogUrl { get; set; }

        [NotMapped]
        public int UnreadArticleCount { get; set; }

        // Navigation properties
        public List<Article> Articles { get; set; } = new List<Article>();

    }
}