namespace Shared.Helpers
{
    /// <summary>
    /// Centralized registry of error codes for Shared library.
    /// Format: Fe3.Shared-XXXXX
    /// </summary>
    public static class SharedErrorCodes
    {
        // RequestHelper
        public const string GetRequestUnexpected = "Fe3.Shared-00001";
        public const string PostUnexpected = "Fe3.Shared-00002";
    }
}
