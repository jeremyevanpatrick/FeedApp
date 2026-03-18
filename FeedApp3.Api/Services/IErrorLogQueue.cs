using FeedApp3.Shared.Models;

namespace FeedApp3.Api.Services
{
    public interface IErrorLogQueue
    {
        void Enqueue(Error error);
    }
}