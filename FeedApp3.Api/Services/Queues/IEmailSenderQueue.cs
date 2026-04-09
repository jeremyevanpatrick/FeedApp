using FeedApp3.Api.Models;
using System.Threading.Channels;

namespace FeedApp3.Api.Services.Queues
{
    public interface IEmailSenderQueue
    {
        void Enqueue(EmailDetails emailDetails);

        ChannelReader<EmailDetails> Reader { get; }
    }
}