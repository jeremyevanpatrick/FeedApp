using System.ComponentModel.DataAnnotations;

namespace FeedApp3.Shared.Services.Requests
{
    public class CreateFeedRequest
    {
        public CreateFeedRequest(string feedUrl)
        {
            FeedUrl = feedUrl;
        }

        [Required]
        [Url]
        public string FeedUrl { get; set; }
    }
}
