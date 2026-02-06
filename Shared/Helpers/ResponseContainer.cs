namespace Shared.Helpers
{
    public class ResponseContainer<T>
    {
        public bool IsSuccess { get; }
        public T? Value { get; }
        public int StatusCode { get; }
        public string? Error { get; }

        private ResponseContainer(bool isSuccess, T? value, int statusCode, string? error)
        {
            IsSuccess = isSuccess;
            Value = value;
            StatusCode = statusCode;
            Error = error;
        }

        public static ResponseContainer<T> Success(T value)
            => new(true, value, 200, null);

        public static ResponseContainer<T> Failure(int statusCode, string? error)
            => new(false, default, statusCode, error);
    }

}
