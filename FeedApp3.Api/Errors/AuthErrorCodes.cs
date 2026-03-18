namespace FeedApp3.Api.Errors
{
    /// <summary>
    /// Centralized registry of error codes for AuthController.
    /// Format: Fe3.Api.Auth-XXXXX
    /// </summary>
    public static class AuthErrorCodes
    {
        // Registration
        public const string RegisterUnexpected = "Fe3.Api.Auth-00001";

        // Resend confirmation email
        public const string ResendConfirmationEmailUnexpected = "Fe3.Api.Auth-00002";

        // Confirm email
        public const string ConfirmEmailUnexpected = "Fe3.Api.Auth-00003";

        // Login
        public const string LoginUnexpected = "Fe3.Api.Auth-00004";

        // Forgot password
        public const string ForgotPasswordUnexpected = "Fe3.Api.Auth-00005";

        // Reset password
        public const string ResetPasswordUnexpected = "Fe3.Api.Auth-00006";

        // Change password
        public const string ChangePasswordUnexpected = "Fe3.Api.Auth-00007";
        public const string ChangePasswordFailed = "Fe3.Api.Auth-00008";

        // Change email
        public const string ChangeEmailUnexpected = "Fe3.Api.Auth-00009";

        // Confirm email change
        public const string ChangeEmailConfirmationUnexpected = "Fe3.Api.Auth-00010";

        // Refresh token
        public const string RefreshTokenUnexpected = "Fe3.Api.Auth-00011";

        // Logout
        public const string LogoutUnexpected = "Fe3.Api.Auth-00012";

        // Delete account
        public const string DeleteAccountUnexpected = "Fe3.Api.Auth-00013";
        public const string DeleteAccountFailed = "Fe3.Api.Auth-00014";
    }
}
