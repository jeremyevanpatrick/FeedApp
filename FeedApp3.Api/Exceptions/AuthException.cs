using Shared.Helpers;

namespace FeedApp3.Api.Exceptions
{
    public class AuthException : Exception
    {
        public int StatusCode { get; }
        public ResponseErrorCodes ErrorCode { get; }

        public AuthException(string message, int statusCode, ResponseErrorCodes errorCode)
            : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }
    }
}
