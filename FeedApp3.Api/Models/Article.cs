using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FeedApp3.Api.Models
{
    public class Article
    {
        [Key]
        public Guid ArticleId { get; set; }
        [Required]
        public Guid FeedId { get; set; }
        [Required]
        public string ArticleUrl { get; set; }
        [Required]
        public string ArticleTitle { get; set; }
        [Required]
        public string ArticleContent { get; set; }
        [Required]
        public bool IsUnread { get; set; }
        [Required]
        public DateTime ArticleDate { get; set; }

        // Navigation properties
        [ForeignKey(nameof(FeedId))]
        public Feed Feed { get; set; } = null!;
    }
}