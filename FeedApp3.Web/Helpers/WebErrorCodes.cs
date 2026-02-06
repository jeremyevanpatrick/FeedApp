namespace FeedApp3.Web.Helpers
{
    /// <summary>
    /// Centralized registry of error codes for Web application.
    /// Format: Fe3.Web.XXXX-XXXXX
    /// </summary>
    public static class WebErrorCodes
    {
        // Controller level exception
        public const string ControllerUnexpected = "Fe3.Web.Controller-00001";

        // RefreshTokenAsync
        public const string RefreshTokenUnexpected = "Fe3.Web.Auth-00001";

    }
}
