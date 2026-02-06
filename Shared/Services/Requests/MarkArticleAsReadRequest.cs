using System.ComponentModel.DataAnnotations;

namespace FeedApp3.Shared.Services.Requests
{
    public class MarkArticleAsReadRequest
    {
        public MarkArticleAsReadRequest(Guid? articleId)
        {
            ArticleId = articleId;
        }

        [Required]
        public Guid? ArticleId { get; set; }
    }
}
