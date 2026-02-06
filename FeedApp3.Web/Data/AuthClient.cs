using FeedApp3.Shared.Data;
using FeedApp3.Shared.Helpers;
using FeedApp3.Shared.Services.Requests;
using FeedApp3.Shared.Services.Responses;
using FeedApp3.Web.Services;
using FeedApp3.Web.Services.Requests;
using FeedApp3.Web.Settings;
using Microsoft.Extensions.Options;

namespace FeedApp3.Web.Data
{
    public class AuthClient : IAuthClient
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly RequestHelper _requestHelperPublic;
        private readonly RequestHelper _requestHelperAuthenticated;
        private readonly string _authBaseUrl;

        public AuthClient(
            IHttpContextAccessor httpContextAccessor,
            PublicHttpClient publicHttpClient,
            AuthenticatedHttpClient authenticatedHttpClient,
            IOptions<ApplicationSettings> applicationSettings,
            ILogger<FeedClient> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _requestHelperPublic = new RequestHelper(publicHttpClient.Client);
            _requestHelperAuthenticated = new RequestHelper(authenticatedHttpClient.Client);
            _authBaseUrl = applicationSettings.Value.AuthBaseUrl;
        }

        public async Task<AuthResponse> LoginAsync(string email, string password)
        {
            string requestUrl = $"{_authBaseUrl}/auth/login";

            LoginWebRequest request = new LoginWebRequest(email, password);
            return await _requestHelperPublic.PostAsync<LoginWebRequest, AuthResponse>(requestUrl, request, null, 9000);

        }

        public async Task RegisterAsync(string email, string password)
        {
            string requestUrl = $"{_authBaseUrl}/auth/register";
            RegisterWebRequest request = new RegisterWebRequest(email, password);
            await _requestHelperPublic.PostAsync(requestUrl, request, null, 9000);
        }

        public async Task ResendConfirmationEmailAsync(string email)
        {
            string requestUrl = $"{_authBaseUrl}/auth/resendconfirmationemail";
            ResendConfirmationEmailRequest request = new ResendConfirmationEmailRequest(email);
            await _requestHelperPublic.PostAsync(requestUrl, request, null, 9000);
        }

        public async Task ConfirmEmailAsync(string userId, string token)
        {
            string requestUrl = $"{_authBaseUrl}/auth/confirmemail";
            ConfirmEmailRequest request = new ConfirmEmailRequest(userId, token);
            await _requestHelperPublic.PostAsync(requestUrl, request, null, 9000);
        }

        public async Task ChangeEmailAsync(string newEmail, string password)
        {
            string requestUrl = $"{_authBaseUrl}/auth/changeemail";
            ChangeEmailRequest request = new ChangeEmailRequest(newEmail, password);
            await _requestHelperAuthenticated.PostAsync(requestUrl, request, null, 9000);
        }

        public async Task ChangeEmailConfirmationAsync(string userId, string newEmail, string token)
        {
            string requestUrl = $"{_authBaseUrl}/auth/changeemailconfirmation";
            ChangeEmailConfirmationRequest request = new ChangeEmailConfirmationRequest(userId, newEmail, token);
            await _requestHelperPublic.PostAsync(requestUrl, request, null, 9000);
        }

        public async Task ChangePasswordAsync(string existingPassword, string newPassword)
        {
            string requestUrl = $"{_authBaseUrl}/auth/changepassword";
            ChangePasswordRequest request = new ChangePasswordRequest(existingPassword, newPassword);
            await _requestHelperAuthenticated.PostAsync(requestUrl, request, null, 9000);
        }

        public async Task ForgotPasswordAsync(string email)
        {
            string requestUrl = $"{_authBaseUrl}/auth/forgotpassword";
            ForgotPasswordRequest request = new ForgotPasswordRequest(email);
            await _requestHelperPublic.PostAsync(requestUrl, request, null, 9000);
        }

        public async Task ResetPasswordAsync(string email, string resetCode, string newPassword)
        {
            string requestUrl = $"{_authBaseUrl}/auth/resetpassword";
            ResetPasswordRequest request = new ResetPasswordRequest(email, resetCode, newPassword);
            await _requestHelperPublic.PostAsync(requestUrl, request, null, 9000);
        }

        public async Task DeleteAccountAsync(string password)
        {
            string requestUrl = $"{_authBaseUrl}/auth/delete";
            DeleteAccountRequest request = new DeleteAccountRequest(password);
            await _requestHelperAuthenticated.PostAsync(requestUrl, request, null, 9000);
        }

        public async Task LogoutAsync()
        {
            string requestUrl = $"{_authBaseUrl}/auth/logout";

            Dictionary<string, string>? headers = null;
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                var refreshToken = httpContext.Request.Cookies["refresh_token"];
                if (!string.IsNullOrWhiteSpace(refreshToken))
                {
                    headers = new Dictionary<string, string>
                    {
                        { "Cookie", $"refresh_token={refreshToken}" }
                    };
                }
            }
            await _requestHelperAuthenticated.PostAsync<object>(requestUrl, null, headers, 9000);
        }

    }
}