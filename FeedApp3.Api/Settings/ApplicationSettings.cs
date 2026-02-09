namespace FeedApp3.Api.Settings
{
    public class ApplicationSettings
    {
        public int MinimumActiveUserRefreshIntervalInMinutes { get; set; }
        public int FeedUpdateBatchSize { get; set; }
        public int FeedUpdateMaxParallelism { get; set; }
    }
}
