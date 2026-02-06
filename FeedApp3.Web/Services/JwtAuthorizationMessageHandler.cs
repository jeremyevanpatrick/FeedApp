using FeedApp3.Shared.Helpers;
using FeedApp3.Shared.Services.Responses;
using FeedApp3.Web.Helpers;
using FeedApp3.Web.Settings;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace FeedApp3.Web.Services
{
    public class JwtAuthorizationMessageHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<JwtAuthorizationMessageHandler> _logger;
        private readonly string _authBaseUrl;
        private static readonly SemaphoreSlim _refreshSemaphore = new SemaphoreSlim(1, 1);

        public JwtAuthorizationMessageHandler(
            IHttpContextAccessor httpContextAccessor,
            ILogger<JwtAuthorizationMessageHandler> logger,
            IOptions<ApplicationSettings> applicationSettings)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _authBaseUrl = applicationSettings.Value.AuthBaseUrl;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var jwtToken = httpContext.Request.Cookies["jwt_token"];

            if (IsTokenExpiredOrExpiring(jwtToken))
            {
                await _refreshSemaphore.WaitAsync(cancellationToken);
                try
                {
                    jwtToken = httpContext.Request.Cookies["jwt_token"];
                    if (IsTokenExpiredOrExpiring(jwtToken))
                    {
                        jwtToken = await RefreshTokenAsync(httpContext, cancellationToken);
                    }
                }
                finally
                {
                    _refreshSemaphore.Release();
                }
            }

            if (!string.IsNullOrWhiteSpace(jwtToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await _refreshSemaphore.WaitAsync(cancellationToken);
                try
                {
                    jwtToken = await RefreshTokenAsync(httpContext, cancellationToken);

                    if (!string.IsNullOrWhiteSpace(jwtToken))
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                        response = await base.SendAsync(request, cancellationToken);
                    }
                }
                finally
                {
                    _refreshSemaphore.Release();
                }
            }

            return response;
        }

        private bool IsTokenExpiredOrExpiring(string jwtToken)
        {
            if (string.IsNullOrWhiteSpace(jwtToken))
            {
                return true;
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = handler.ReadJwtToken(jwtToken);

                return jwtSecurityToken.ValidTo <= DateTime.UtcNow.AddMinutes(2);
            }
            catch
            {
                return true;
            }
        }

        private async Task<string?> RefreshTokenAsync(HttpContext httpContext, CancellationToken cancellationToken)
        {
            var refreshToken = httpContext.Request.Cookies["refresh_token"];

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return null;
            }

            try
            {
                using var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(_authBaseUrl);
                httpClient.DefaultRequestHeaders.Add("Cookie", $"refresh_token={refreshToken}");

                var response = await httpClient.PostAsync("/auth/refresh", null, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    httpContext.Response.Cookies.Append("refresh_token", authResponse.RefreshToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = authResponse.RefreshTokenExpiresAt
                    });

                    httpContext.Response.Cookies.Append("jwt_token", authResponse.AccessToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = authResponse.AccessTokenExpiresAt
                    });

                    return authResponse.AccessToken;
                }
                else
                {
                    httpContext.Response.Cookies.Delete("jwt_token");
                    httpContext.Response.Cookies.Delete("refresh_token");
                    return null;
                }
            }
            catch(Exception ex)
            {
                _logger.LogErrorWithDictionary(WebErrorCodes.RefreshTokenUnexpected, ex, "Unexpected error while refreshing token");
                httpContext.Response.Cookies.Delete("jwt_token");
                httpContext.Response.Cookies.Delete("refresh_token");
                return null;
            }
        }
    }
}
