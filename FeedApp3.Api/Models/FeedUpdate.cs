namespace FeedApp3.Api.Models
{
    public class FeedUpdate
    {
        public Guid FeedUpdateId { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }

        public DateTime RequestedAt { get; set; }
    }
}
