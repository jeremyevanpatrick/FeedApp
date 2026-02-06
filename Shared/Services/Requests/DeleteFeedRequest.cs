using System.ComponentModel.DataAnnotations;

namespace FeedApp3.Shared.Services.Requests
{
    public class DeleteFeedRequest
    {
        public DeleteFeedRequest(Guid? feedId)
        {
            FeedId = feedId;
        }

        [Required]
        public Guid? FeedId { get; set; }
    }
}
