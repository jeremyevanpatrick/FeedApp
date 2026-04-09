using FeedApp3.Shared.Models;
using System.Threading.Channels;

namespace FeedApp3.Shared.Services.Queues
{
    public interface ILogProcessorQueue
    {
        void Enqueue(ApplicationLog applicationLog);

        ChannelReader<ApplicationLog> Reader { get; }
    }
}