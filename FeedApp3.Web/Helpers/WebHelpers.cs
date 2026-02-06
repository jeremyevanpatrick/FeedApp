namespace FeedApp3.Web.Helpers
{
    public static class WebHelpers
    {
        public static bool IsAjaxRequest(HttpRequest request)
        {
            //AJAX request
            if (request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return true;
            }

            return false;
        }
    }
}
