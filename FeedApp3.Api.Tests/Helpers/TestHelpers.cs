using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace FeedApp3.Api.Tests.Helpers
{
    public static class TestHelpers
    {
        private static string? GetCookieString(ControllerBase controller)
        {
            var headers = controller.ControllerContext.HttpContext.Response.Headers;
            headers.Should().ContainKey("Set-Cookie");
            return headers["Set-Cookie"].ToString();
        } 

        public static void HasCookie(ControllerBase controller, string cookieKeyValue)
        {
            GetCookieString(controller).Should().Contain(cookieKeyValue);
        }

        public static void NotHaveCookie(ControllerBase controller, string cookieKeyValue)
        {
            GetCookieString(controller).Should().NotContain(cookieKeyValue);
        }
    }
}
