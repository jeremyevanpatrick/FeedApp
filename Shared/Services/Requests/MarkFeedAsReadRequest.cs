using System.ComponentModel.DataAnnotations;

namespace FeedApp3.Shared.Services.Requests
{
    public class MarkFeedAsReadRequest
    {
        public MarkFeedAsReadRequest(Guid? feedId)
        {
            FeedId = feedId;
        }

        [Required]
        public Guid? FeedId { get; set; }
    }
}
