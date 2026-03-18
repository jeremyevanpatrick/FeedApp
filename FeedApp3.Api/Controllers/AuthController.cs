using FeedApp3.Api.Errors;
using FeedApp3.Api.Exceptions;
using FeedApp3.Api.Extensions;
using FeedApp3.Api.Services.Application;
using FeedApp3.Shared.Helpers;
using FeedApp3.Shared.Services.Requests;
using FeedApp3.Shared.Services.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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

        [EnableRateLimiting("public-messaging-endpoints")]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
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
                _logger.LogErrorWithDictionary(AuthErrorCodes.RegisterUnexpected, ex, "Unexpected error during user registration", new Dictionary<string, string> { });
                return Problem500();
            }
        }

        [EnableRateLimiting("public-messaging-endpoints")]
        [HttpPost("resendconfirmationemail")]
        public async Task<IActionResult> ResendConfirmationEmail(ResendConfirmationEmailRequest request)
        {
            try
            {
                await _authAppService.ResendConfirmationEmailAsync(request.Email);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(AuthErrorCodes.ResendConfirmationEmailUnexpected, ex, "Unexpected error while resending confirmation email", new Dictionary<string, string> { });
                return Ok();
            }
        }

        [HttpPost("confirmemail")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
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
                _logger.LogErrorWithDictionary(AuthErrorCodes.ConfirmEmailUnexpected, ex, "Unexpected error while confirming email", new Dictionary<string, string> { });
                return Problem500();
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
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
                _logger.LogErrorWithDictionary(AuthErrorCodes.LoginUnexpected, ex, "Unexpected error during user login", new Dictionary<string, string> { });
                return Problem500();
            }
        }

        [EnableRateLimiting("public-messaging-endpoints")]
        [HttpPost("forgotpassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                await _authAppService.ForgotPasswordAsync(request.Email);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(AuthErrorCodes.ForgotPasswordUnexpected, ex, "Unexpected error while sending reset password email", new Dictionary<string, string> { });
                return Ok();
            }
        }

        [HttpPost("resetpassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
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
                _logger.LogErrorWithDictionary(AuthErrorCodes.ResetPasswordUnexpected, ex, "Unexpected error while resetting password", new Dictionary<string, string> { });
                return Problem500();
            }
        }

        [HttpPost("changepassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
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
                _logger.LogErrorWithDictionary(AuthErrorCodes.ChangePasswordUnexpected, ex, "Unexpected error while changing password", new Dictionary<string, string>
                {
                    { "UserId", userId }
                });
                return Problem500();
            }
        }

        [HttpPost("changeemail")]
        [Authorize]
        public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailRequest request)
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
                _logger.LogErrorWithDictionary(AuthErrorCodes.ChangeEmailUnexpected, ex, "Unexpected error while sending change email message", new Dictionary<string, string>
                {
                    { "UserId", userId }
                });
                return Problem500();
            }
        }

        [HttpPost("changeemailconfirmation")]
        public async Task<IActionResult> ChangeEmailConfirmation([FromBody] ChangeEmailConfirmationRequest request)
        {
            string userId = string.Empty;

            try
            {
                userId = User.GetUserId().ToString();
                await _authAppService.ChangeEmailConfirmationAsync(userId, request.NewEmail, request.Token);
                return Ok();
            }
            catch (AuthException ex)
            {
                return ProblemWithErrorCode(ex.StatusCode, ex.Message, ex.ErrorCode);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(AuthErrorCodes.ChangeEmailConfirmationUnexpected, ex, "Unexpected error while confirming email change", new Dictionary<string, string>
                {
                    { "UserId", userId }
                });
                return Problem500();
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken()
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
                _logger.LogErrorWithDictionary(AuthErrorCodes.RefreshTokenUnexpected, ex, "Unexpected error while refreshing token", new Dictionary<string, string>{});
                return Problem500();
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var refreshTokenString = Request.Cookies["refresh_token"];
                await _authAppService.LogoutAsync(refreshTokenString);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(AuthErrorCodes.LogoutUnexpected, ex, "Unexpected error while signing out", new Dictionary<string, string> { });
                return Problem500();
            }
        }

        [HttpPost("delete")]
        [Authorize]
        public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequest request)
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
                _logger.LogErrorWithDictionary(AuthErrorCodes.DeleteAccountUnexpected, ex, "Unexpected error while deleting account", new Dictionary<string, string>
                {
                    { "UserId", userId }
                });
                return Problem500();
            }
        }

    }
}
