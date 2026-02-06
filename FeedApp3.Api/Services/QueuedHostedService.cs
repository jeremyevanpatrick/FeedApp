using FeedApp3.Api.Helpers;
using FeedApp3.Shared.Helpers;

namespace FeedApp3.Api.Services
{
    public class QueuedHostedService : BackgroundService
    {
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly ILogger<QueuedHostedService> _logger;

        public QueuedHostedService(
            IBackgroundTaskQueue taskQueue,
            ILogger<QueuedHostedService> logger)
        {
            _taskQueue = taskQueue;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var workItem = await _taskQueue.DequeueAsync(stoppingToken);

                    await workItem(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogErrorWithDictionary(FeedErrorCodes.BackgroundTaskUnexpected, ex, "Unexpected error while processing background task");
                }
            }
        }
    }
}
