using System.ComponentModel.DataAnnotations;

namespace FeedApp3.Web.Models
{
    public class AddFeedModel
    {
        [Required]
        [Url]
        public string? FeedUrl { get; set; }
    }
}
