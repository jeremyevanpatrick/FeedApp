using FeedApp3.Shared.Data;
using FeedApp3.Shared.Services.Responses;
using FeedApp3.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FeedApp3.Web.Controllers
{
    public class LoginController : Controller
    {
        private readonly IAuthClient _authClient;
        private readonly ILogger<LoginController> _logger;

        public LoginController(IAuthClient authClient, ILogger<LoginController> logger)
        {
            _authClient = authClient;
            _logger = logger;
        }

        [HttpGet]
        public ViewResult Login(string? message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                ViewBag.ErrorMessage = message;
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromForm] LoginModel model)
        {
            AuthResponse authResponse = await _authClient.LoginAsync(model.LoginEmail, model.LoginPassword);

            Response.Cookies.Append("refresh_token", authResponse.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = authResponse.RefreshTokenExpiresAt
            });

            Response.Cookies.Append("jwt_token", authResponse.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = authResponse.AccessTokenExpiresAt
            });

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, model.LoginEmail),
                new Claim(ClaimTypes.Email, model.LoginEmail)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = false });

            return RedirectToAction("Feeds", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([FromForm] RegisterModel model)
        {
            await _authClient.RegisterAsync(model.RegisterEmail, model.RegisterPassword);
            return Ok();
        }

        [HttpGet]
        public async Task<ViewResult> ConfirmEmail([FromQuery] string userId, string token)
        {
            await _authClient.ConfirmEmailAsync(userId, token);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendConfirmationEmail([FromForm] ResendEmailConfirmationModel model)
        {
            await _authClient.ResendConfirmationEmailAsync(model.Email);
            return Ok();
        }

        [HttpGet]
        public ViewResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword([FromForm] ForgotPasswordModel model)
        {
            await _authClient.ForgotPasswordAsync(model.Email);
            return Ok();
        }
        
        [HttpGet]
        public ViewResult ResetPassword([FromQuery] string email, string token)
        {
            var model = new ResetPasswordModel()
            {
                Email = email,
                Token = token
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword([FromForm] ResetPasswordModel model)
        {
            await _authClient.ResetPasswordAsync(model.Email, model.Token, model.NewPassword);
            return Ok();
        }

        [HttpGet]
        public async Task<ViewResult> ConfirmEmailChange([FromQuery] string userId, string email, string token)
        {
            await _authClient.ChangeEmailConfirmationAsync(userId, email, token);
            return View();
        }

    }
}
