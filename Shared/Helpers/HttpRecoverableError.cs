using System.Net;

namespace FeedApp3.Shared.Helpers
{
    public class HttpRecoverableError : Exception
    {
        public HttpStatusCode HttpStatusCode { get; }

        public HttpRecoverableError(string message, HttpStatusCode httpStatusCode)
            : base(message)
        {
            HttpStatusCode = httpStatusCode;
        }

    }
}
