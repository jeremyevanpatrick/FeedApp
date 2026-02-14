using System.Net;

namespace FeedApp3.Shared.Helpers
{
    public class HttpRecoverableError : Exception
    {
        public HttpStatusCode HttpStatusCode { get; }
        public bool IsUnauthorized { get; }

        public HttpRecoverableError(string message, HttpStatusCode httpStatusCode, bool isUnauthorized = false)
            : base(message)
        {
            HttpStatusCode = httpStatusCode;
            IsUnauthorized = isUnauthorized;
        }

    }
}
