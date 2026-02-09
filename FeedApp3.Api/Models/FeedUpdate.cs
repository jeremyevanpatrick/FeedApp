using System.ComponentModel.DataAnnotations;

namespace FeedApp3.Api.Models
{
    public class FeedUpdate
    {
        [Key]
        public Guid FeedUpdateId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public DateTime RequestedAt { get; set; }
    }
}
