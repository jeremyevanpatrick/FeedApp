using FeedApp3.Shared.Models;
using FeedApp3.Shared.Services.Queues;
using FeedApp3.Shared.Settings;
using Microsoft.Extensions.Options;

namespace FeedApp3.Web.Services
{
    public class LogSenderService : BackgroundService
    {
        private readonly ILogProcessorQueue _queue;
        private readonly HttpClient _httpClient;
        private readonly RemoteLoggingSettings _remoteLoggingSettings;

        private readonly int MaxBatchSize = 50;
        private readonly TimeSpan MaxBatchWait = TimeSpan.FromSeconds(5);

        public LogSenderService(
            ILogProcessorQueue queue,
            HttpClient httpClient,
            IOptions<RemoteLoggingSettings> remoteLoggingSettings)
        {
            _queue = queue;
            _httpClient = httpClient;
            _remoteLoggingSettings = remoteLoggingSettings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken = default)
        {
            var batch = new List<ApplicationLog>(MaxBatchSize);

            while (!stoppingToken.IsCancellationRequested)
            {
                if (await _queue.Reader.WaitToReadAsync(stoppingToken))
                {
                    while (_queue.Reader.TryRead(out var item))
                    {
                        batch.Add(item);

                        if (batch.Count >= MaxBatchSize)
                        {
                            await FlushBatch(batch);
                            batch.Clear();
                        }
                    }

                    if (batch.Count > 0)
                    {
                        await Task.Delay(MaxBatchWait, stoppingToken);
                        await FlushBatch(batch);
                        batch.Clear();
                    }
                }
            }
        }

        private async Task FlushBatch(List<ApplicationLog> batch)
        {
            if (batch.Count == 0) return;

            try
            {
                await _httpClient.PostAsJsonAsync(_remoteLoggingSettings.Endpoint, batch);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
