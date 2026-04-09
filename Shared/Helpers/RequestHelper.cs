using FeedApp3.Shared.Errors;
using Shared.Exceptions;
using Shared.Services.Responses;
using System.Net;
using System.Net.Http.Json;

namespace FeedApp3.Shared.Helpers
{
    public class RequestHelper
    {
        private readonly HttpClient _httpClient;

        public RequestHelper(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<TResponse?> GetAsync<TResponse>(string requestUrl, IDictionary<string, string>? headers = null, int timeoutMs = 3000)
        {
            var response = await SendRequestAsync<object>(HttpMethod.Get, requestUrl, null, headers, timeoutMs);
            return await response.Content.ReadFromJsonAsync<TResponse>();
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string requestUrl, TRequest? request, IDictionary<string, string>? headers = null, int timeoutMs = 3000)
        {
            var response = await SendRequestAsync<TRequest>(HttpMethod.Post, requestUrl, request, headers, timeoutMs);
            return await response.Content.ReadFromJsonAsync<TResponse>();
        }

        public async Task PostAsync<TRequest>(string requestUrl, TRequest? request, IDictionary<string, string>? headers = null, int timeoutMs = 3000)
        {
            await SendRequestAsync<TRequest>(HttpMethod.Post, requestUrl, request, headers, timeoutMs);
        }

        private async Task<HttpResponseMessage> SendRequestAsync<TRequest>(HttpMethod method, string requestUrl, TRequest? request, IDictionary<string, string>? headers = null, int timeoutMs = 3000)
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeoutMs));

            var httpRequestMessage = new HttpRequestMessage(method, requestUrl);
            if (request != null)
            {
                httpRequestMessage.Content = JsonContent.Create<TRequest>(request);
            }

            AddHeaders(httpRequestMessage, headers);

            var response = await _httpClient.SendAsync(httpRequestMessage, cts.Token);

            HandleErrors(response);

            return response;
        }

        private void AddHeaders(HttpRequestMessage request, IDictionary<string, string>? headers)
        {
            if (headers == null)
            {
                return;
            }

            foreach (var header in headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }
        }

        private void HandleErrors(HttpResponseMessage response)
        {
            //check for recoverable error
            if (response.StatusCode >= HttpStatusCode.BadRequest &&
                response.StatusCode <= (HttpStatusCode)499)
            {
                ApiErrorResponse? apiErrorResponse = null;
                try
                {
                    apiErrorResponse = response.Content.ReadFromJsonAsync<ApiErrorResponse>().Result;
                }
                catch { }

                if (!string.IsNullOrWhiteSpace(apiErrorResponse?.ErrorCode) && RecoverableErrorCodes.Any(e=> e.ToString() == apiErrorResponse.ErrorCode) && !string.IsNullOrWhiteSpace(apiErrorResponse?.Detail))
                {
                    bool isUnauthorized = ApiErrorCodes.UNAUTHORIZED.ToString() == apiErrorResponse.ErrorCode;
                    throw new HttpRecoverableError(apiErrorResponse.Detail, response.StatusCode, isUnauthorized);
                }
            }
            //check for unrecoverable error
            response.EnsureSuccessStatusCode();
        }

        private List<string> RecoverableErrorCodes { get; } = new ()
        {
            ApiErrorCodes.INVALID_CREDENTIALS,
            ApiErrorCodes.INVALID_REQUEST_PARAMETERS,
            ApiErrorCodes.TOKEN_INVALID_OR_EXPIRED,
            ApiErrorCodes.PASSWORD_DOES_NOT_MEET_REQUIREMENTS,
            ApiErrorCodes.EMAIL_ADDRESS_ALREADY_IN_USE,
            ApiErrorCodes.ACCOUNT_LOCKED,
            ApiErrorCodes.AUTH_NO_LONGER_VALID,
            ApiErrorCodes.UNAUTHORIZED,
            ApiErrorCodes.FORBIDDEN,
            ApiErrorCodes.TOOMANYREQUESTS
        };

    }

}
