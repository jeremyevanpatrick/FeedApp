namespace FeedApp3.Api.Errors
{
    /// <summary>
    /// Centralized registry of error codes for FeedUpdateService.
    /// Format: Fe3.Api.FUS-XXXXX
    /// </summary>
    public static class FeedUpdateErrorCodes
    {
        // Feed update service (background task)
        public const string UpdateFeedInBackgroundUnexpected = "Fe3.Api.FUS-00001";
        public const string UpdateFeedNotFound = "Fe3.Api.FUS-00002";
        public const string ProcessFeedUpdatesUnexpected = "Fe3.Api.FUS-00003";
        public const string ProcessSingleUserFeedsUnexpected = "Fe3.Api.FUS-00004";
    }
}
