using System.ComponentModel.DataAnnotations;

namespace FeedApp3.Web.Models
{
    public class DeleteFeedModel
    {
        [Required]
        public Guid FeedId { get; set; }
    }
}
