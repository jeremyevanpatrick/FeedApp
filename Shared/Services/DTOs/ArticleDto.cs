using System.ComponentModel.DataAnnotations;

namespace FeedApp3.Shared.Services.DTOs
{
    public class ArticleDto
    {
        [Required]
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

    }
}