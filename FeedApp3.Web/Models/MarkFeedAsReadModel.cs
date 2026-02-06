using System.ComponentModel.DataAnnotations;

namespace FeedApp3.Web.Models
{
    public class MarkFeedAsReadModel
    {
        [Required]
        public Guid FeedId { get; set; }
    }
}
