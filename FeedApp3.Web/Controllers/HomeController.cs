using FeedApp3.Shared.Data;
using FeedApp3.Shared.Services.DTOs;
using FeedApp3.Web.Helpers;
using FeedApp3.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FeedApp3.Shared.Helpers;

namespace FeedApp3.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IFeedClient _feedClient;
        private readonly IAuthClient _authClient;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IFeedClient feedClient, IAuthClient authClient, ILogger<HomeController> logger)
        {
            _feedClient = feedClient;
            _authClient = authClient;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ViewResult> Feeds()
        {
            List<FeedDto> feedList = await _feedClient.GetFeedList();
            return View(feedList);
        }

        [HttpGet]
        public async Task<ViewResult> Articles(Guid feedId)
        {
            FeedDto feed = await _feedClient.GetFeedById(feedId);
            return View(feed);
        }

        [HttpGet]
        public async Task<ViewResult> Account()
        {
            ViewBag.Username = User?.Identity?.Name;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFeed([FromForm] AddFeedModel model)
        {
            await _feedClient.Create(model.FeedUrl);
            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFeed([FromBody] DeleteFeedModel model)
        {
            await _feedClient.Delete(model.FeedId);
            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkFeedAsRead([FromBody] MarkFeedAsReadModel model)
        {
            await _feedClient.MarkFeedAsRead(model.FeedId);
            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkArticleAsRead([FromBody] MarkArticleAsReadModel model)
        {
            await _feedClient.MarkArticleAsRead(model.ArticleId);
            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeEmail([FromForm] ChangeEmailModel model)
        {
            await _authClient.ChangeEmailAsync(model.NewEmail, model.Password);
            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword([FromForm] ChangePasswordModel model)
        {
            await _authClient.ChangePasswordAsync(model.ExistingPassword, model.NewPassword);
            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount([FromForm] DeleteAccountModel model)
        {
            await _authClient.DeleteAccountAsync(model.DeleteAccountPassword);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Logout(string? message = null)
        {
            try
            {
                //Revoke refresh token from db
                await _authClient.LogoutAsync();

                //Clear tokens from cookies
                Response.Cookies.Delete("jwt_token");
                Response.Cookies.Delete("refresh_token");

                //Logout of MVC app
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(WebErrorCodes.ControllerUnexpected, ex, "Unexpected error while logging out");
            }

            return RedirectToAction("Login", "Login", new { message = message });
        }

    }
}
