namespace FeedApp3.Api.Helpers
{
    /// <summary>
    /// Centralized registry of error codes for DataCleanupService.
    /// Format: Fe3.Api.DCS-XXXXX
    /// </summary>
    public static class CleanupErrorCodes
    {
        // Cleanup refresh tokens
        public const string CleanupRefreshTokensUnexpected = "Fe3.Api.DCS-00001";

        // Cleanup soft deleted users
        public const string CleanupSoftDeletedUsersUnexpected = "Fe3.Api.DCS-00002";
        public const string CleanupSoftDeletedUsersFailed = "Fe3.Api.DCS-00003";

    }
}
