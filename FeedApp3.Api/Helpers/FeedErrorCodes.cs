namespace FeedApp3.Api.Helpers
{
    /// <summary>
    /// Centralized registry of error codes for FeedsController.
    /// Format: Fe3.Api.Feed-XXXXX
    /// </summary>
    public static class FeedErrorCodes
    {
        // Get feed list
        public const string GetFeedListUnexpected = "Fe3.Api.Feed-00001";

        // Get feed by id
        public const string GetFeedByIdUnexpected = "Fe3.Api.Feed-00002";

        // Create feed
        public const string CreateFeedUnexpected = "Fe3.Api.Feed-00003";

        // Update feed
        public const string UpdateFeedUnexpected = "Fe3.Api.Feed-00004";

        // Delete feed
        public const string DeleteFeedUnexpected = "Fe3.Api.Feed-00005";

        // Mark feed as read
        public const string MarkFeedAsReadUnexpected = "Fe3.Api.Feed-00006";

        // Mark article as read
        public const string MarkArticleAsReadUnexpected = "Fe3.Api.Feed-00007";
    }
}
