using FeedApp3.Api.Exceptions;
using FeedApp3.Api.Extensions;
using FeedApp3.Api.Services.Application;
using FeedApp3.Shared.Errors;
using FeedApp3.Shared.Services.Requests;
using FeedApp3.Shared.Services.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using FeedApp3.Shared.Extensions;

namespace FeedApp3.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBaseExtended
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthAppService _authAppService;

        public AuthController(
            ILogger<AuthController> logger,
            IAuthAppService authAppService)
        {
            _logger = logger;
            _authAppService = authAppService;
        }

        [EnableRateLimiting("messaging-endpoints-global")]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            string? correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            using (_logger.BeginLoggingScope(nameof(AuthController), nameof(Register), correlationId))
            {
                try
                {
                    await _authAppService.RegisterAsync(request.Email, request.Password);
                    return Ok();
                }
                catch (AuthException ex)
                {
                    return ProblemWithErrorCode(ex.StatusCode, ex.Message, ex.ErrorCode);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error during user registration. ErrorCode: {ErrorCode}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR);
                    return Problem500();
                }
            }
        }

        [EnableRateLimiting("messaging-endpoints-global")]
        [HttpPost("resendconfirmationemail")]
        public async Task<IActionResult> ResendConfirmationEmail(ResendConfirmationEmailRequest request)
        {
            string? correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            using (_logger.BeginLoggingScope(nameof(AuthController), nameof(ResendConfirmationEmail), correlationId))
            {
                try
                {
                    await _authAppService.ResendConfirmationEmailAsync(request.Email);
                    return Ok();
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while resending confirmation email. ErrorCode: {ErrorCode}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR);
                    return Ok();
                }
            }
        }

        [HttpPost("confirmemail")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
        {
            string? correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            using (_logger.BeginLoggingScope(nameof(AuthController), nameof(ConfirmEmail), correlationId))
            {
                try
                {
                    await _authAppService.ConfirmEmailAsync(request.UserId, request.Token);
                    return Ok();
                }
                catch (AuthException ex)
                {
                    return ProblemWithErrorCode(ex.StatusCode, ex.Message, ex.ErrorCode);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while confirming email. ErrorCode: {ErrorCode}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR);
                    return Problem500();
                }
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            string? correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            using (_logger.BeginLoggingScope(nameof(AuthController), nameof(Login), correlationId))
            {
                try
                {
                    AuthResponse authResponse = await _authAppService.LoginAsync(request.Email, request.Password);
                    return Ok(authResponse);
                }
                catch (AuthException ex)
                {
                    return ProblemWithErrorCode(ex.StatusCode, ex.Message, ex.ErrorCode);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error during user login. ErrorCode: {ErrorCode}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR);
                    return Problem500();
                }
            }
        }

        [EnableRateLimiting("messaging-endpoints-global")]
        [HttpPost("forgotpassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            string? correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            using (_logger.BeginLoggingScope(nameof(AuthController), nameof(ForgotPassword), correlationId))
            {
                try
                {
                    await _authAppService.ForgotPasswordAsync(request.Email);
                    return Ok();
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while sending reset password email. ErrorCode: {ErrorCode}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR);
                    return Ok();
                }
            }
        }

        [HttpPost("resetpassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            string? correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            using (_logger.BeginLoggingScope(nameof(AuthController), nameof(ResetPassword), correlationId))
            {
                try
                {
                    await _authAppService.ResetPasswordAsync(request.Email, request.ResetCode, request.NewPassword);
                    return Ok();
                }
                catch (AuthException ex)
                {
                    return ProblemWithErrorCode(ex.StatusCode, ex.Message, ex.ErrorCode);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while resetting password. ErrorCode: {ErrorCode}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR);
                    return Problem500();
                }
            }
        }

        [HttpPost("changepassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            string? correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            using (_logger.BeginLoggingScope(nameof(AuthController), nameof(ChangePassword), correlationId))
            {
                string userId = string.Empty;

                try
                {
                    userId = User.GetUserId().ToString();
                    await _authAppService.ChangePasswordAsync(userId, request.ExistingPassword, request.NewPassword);
                    return Ok();
                }
                catch (AuthException ex)
                {
                    return ProblemWithErrorCode(ex.StatusCode, ex.Message, ex.ErrorCode);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while changing password. ErrorCode: {ErrorCode}, UserId: {UserId}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR,
                        userId);
                    return Problem500();
                }
            }
        }

        [EnableRateLimiting("messaging-endpoints-global")]
        [HttpPost("changeemail")]
        [Authorize]
        public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailRequest request)
        {
            string? correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            using (_logger.BeginLoggingScope(nameof(AuthController), nameof(ChangeEmail), correlationId))
            {
                string userId = string.Empty;

                try
                {
                    userId = User.GetUserId().ToString();
                    await _authAppService.ChangeEmailAsync(userId, request.NewEmail, request.Password);
                    return Ok();
                }
                catch (AuthException ex)
                {
                    return ProblemWithErrorCode(ex.StatusCode, ex.Message, ex.ErrorCode);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while sending change email message. ErrorCode: {ErrorCode}, UserId: {UserId}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR,
                        userId);
                    return Problem500();
                }
            }
        }

        [HttpPost("changeemailconfirmation")]
        public async Task<IActionResult> ChangeEmailConfirmation([FromBody] ChangeEmailConfirmationRequest request)
        {
            string? correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            using (_logger.BeginLoggingScope(nameof(AuthController), nameof(ChangeEmailConfirmation), correlationId))
            {
                try
                {
                    await _authAppService.ChangeEmailConfirmationAsync(request.UserId, request.NewEmail, request.Token);
                    return Ok();
                }
                catch (AuthException ex)
                {
                    return ProblemWithErrorCode(ex.StatusCode, ex.Message, ex.ErrorCode);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while confirming email change. ErrorCode: {ErrorCode}, UserId: {UserId}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR,
                        request.UserId);
                    return Problem500();
                }
            }
        }

        [EnableRateLimiting("public-token-refresh-endpoint")]
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken()
        {
            string? correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            using (_logger.BeginLoggingScope(nameof(AuthController), nameof(RefreshToken), correlationId))
            {
                try
                {
                    var refreshTokenString = Request.Cookies["refresh_token"];
                    AuthResponse authResponse = await _authAppService.RefreshTokenAsync(refreshTokenString);
                    return Ok(authResponse);
                }
                catch (AuthException ex)
                {
                    return ProblemWithErrorCode(ex.StatusCode, ex.Message, ex.ErrorCode);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while refreshing token. ErrorCode: {ErrorCode}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR);
                    return Problem500();
                }
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            string? correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            using (_logger.BeginLoggingScope(nameof(AuthController), nameof(Logout), correlationId))
            {
                try
                {
                    var refreshTokenString = Request.Cookies["refresh_token"];
                    await _authAppService.LogoutAsync(refreshTokenString);
                    return Ok();
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while signing out. ErrorCode: {ErrorCode}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR);
                    return Problem500();
                }
            }
        }

        [HttpPost("delete")]
        [Authorize]
        public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequest request)
        {
            string? correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            using (_logger.BeginLoggingScope(nameof(AuthController), nameof(DeleteAccount), correlationId))
            {
                string userId = string.Empty;

                try
                {
                    userId = User.GetUserId().ToString();
                    await _authAppService.DeleteAccountAsync(userId, request.Password);
                    return Ok();
                }
                catch (AuthException ex)
                {
                    return ProblemWithErrorCode(ex.StatusCode, ex.Message, ex.ErrorCode);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while deleting account. ErrorCode: {ErrorCode}, UserId: {UserId}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR,
                        userId);
                    return Problem500();
                }
            }
        }

    }
}
