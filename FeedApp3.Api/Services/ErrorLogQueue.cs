using FeedApp3.Shared.Models;
using System.Threading.Channels;

namespace FeedApp3.Api.Services
{
    public class ErrorLogQueue : IErrorLogQueue
    {
        private readonly Channel<Error> _channel;

        public ErrorLogQueue()
        {
            _channel = Channel.CreateUnbounded<Error>();
        }

        public ChannelReader<Error> Reader => _channel.Reader;

        public void Enqueue(Error error)
        {
            _channel.Writer.TryWrite(error);
        }
    }
}
