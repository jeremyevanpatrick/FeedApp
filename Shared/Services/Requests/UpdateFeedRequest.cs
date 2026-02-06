using System.ComponentModel.DataAnnotations;

namespace FeedApp3.Shared.Services.Requests
{
    public class UpdateFeedRequest
    {
        public UpdateFeedRequest(Guid? feedId)
        {
            FeedId = feedId;
        }

        [Required]
        public Guid? FeedId { get; set; }
    }
}
