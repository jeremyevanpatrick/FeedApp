using System.ComponentModel.DataAnnotations;

namespace FeedApp3.Web.Models
{
    public class MarkArticleAsReadModel
    {
        [Required]
        public Guid ArticleId { get; set; }
    }
}
