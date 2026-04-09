using FeedApp3.Api.Models;
using System.Threading.Channels;

namespace FeedApp3.Api.Services.Queues
{
    public class EmailSenderQueue : IEmailSenderQueue
    {
        private readonly Channel<EmailDetails> _channel;

        public EmailSenderQueue()
        {
            _channel = Channel.CreateUnbounded<EmailDetails>();
        }

        public ChannelReader<EmailDetails> Reader => _channel.Reader;

        public void Enqueue(EmailDetails error)
        {
            _channel.Writer.TryWrite(error);
        }
    }
}
