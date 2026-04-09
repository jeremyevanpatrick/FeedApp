using FeedApp3.Api.Controllers;
using FeedApp3.Api.Exceptions;
using FeedApp3.Api.Services.Application;
using FeedApp3.Shared.Errors;
using FeedApp3.Shared.Services.Requests;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Services.Responses;
using System.Security.Claims;

namespace FeedApp3.Api.Tests.Controllers
{
    public class AuthControllerTests
    {
        private static AuthController CreateController(Mock<IAuthAppService> mockService)
        {
            var controller = new AuthController(
                Mock.Of<ILogger<AuthController>>(),
                mockService.Object);

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            return controller;
        }

        private static AuthController CreateControllerWithUser(Guid userId, Mock<IAuthAppService> mockService)
        {
            var controller = new AuthController(
                Mock.Of<ILogger<AuthController>>(),
                mockService.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }, "mock"));

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            return controller;
        }

        [Fact]
        public async Task Register_ValidRequest_ReturnsOkResult()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();

            var controller = CreateController(mockService);

            //Act
            var request = new RegisterRequest { Email = "test", Password = "test" };
            var result = await controller.Register(request);

            //Assert
            result.Should().BeOfType<OkResult>();
        }

        [Fact]
        public async Task Register_NotFound_Returns400Result()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.RegisterAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new AuthException("Test error", StatusCodes.Status400BadRequest, ApiErrorCodes.PASSWORD_DOES_NOT_MEET_REQUIREMENTS));

            var controller = CreateController(mockService);

            //Act
            var request = new RegisterRequest { Email = "test", Password = "test" };
            var result = await controller.Register(request);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(400);
            result.Should().BeOfType<ObjectResult>()
                .Which.Value.Should().BeOfType<ApiErrorResponse>()
                .Which.ErrorCode.Should().Be(ApiErrorCodes.PASSWORD_DOES_NOT_MEET_REQUIREMENTS.ToString());
        }

        [Fact]
        public async Task Register_InternalError_Returns500Result()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.RegisterAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test error"));

            var controller = CreateController(mockService);

            //Act
            var request = new RegisterRequest { Email = "test", Password = "test" };
            var result = await controller.Register(request);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task ResendConfirmationEmail_ValidRequest_ReturnsOkResult()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();

            var controller = CreateController(mockService);

            //Act
            var request = new ResendConfirmationEmailRequest { Email = "test" };
            var result = await controller.ResendConfirmationEmail(request);

            //Assert
            result.Should().BeOfType<OkResult>();
        }

        [Fact]
        public async Task ResendConfirmationEmail_InternalError_Returns500Result()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.ResendConfirmationEmailAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test error"));

            var controller = CreateController(mockService);

            //Act
            var request = new ResendConfirmationEmailRequest { Email = "test" };
            var result = await controller.ResendConfirmationEmail(request);

            //Assert
            result.Should().BeOfType<OkResult>();
        }

        [Fact]
        public async Task ConfirmEmail_ValidRequest_ReturnsOkResult()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();

            var controller = CreateController(mockService);

            //Act
            var request = new ConfirmEmailRequest(Guid.NewGuid().ToString(), "test");
            var result = await controller.ConfirmEmail(request);

            //Assert
            result.Should().BeOfType<OkResult>();
        }

        [Fact]
        public async Task ConfirmEmail_NotFound_Returns400Result()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.ConfirmEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new AuthException("Test error", StatusCodes.Status400BadRequest, ApiErrorCodes.TOKEN_INVALID_OR_EXPIRED));

            var controller = CreateController(mockService);

            //Act
            var request = new ConfirmEmailRequest(Guid.NewGuid().ToString(), "test");
            var result = await controller.ConfirmEmail(request);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(400);
            result.Should().BeOfType<ObjectResult>()
                .Which.Value.Should().BeOfType<ApiErrorResponse>()
                .Which.ErrorCode.Should().Be(ApiErrorCodes.TOKEN_INVALID_OR_EXPIRED.ToString());
        }

        [Fact]
        public async Task ConfirmEmail_InternalError_Returns500Result()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.ConfirmEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test error"));

            var controller = CreateController(mockService);

            //Act
            var request = new ConfirmEmailRequest(Guid.NewGuid().ToString(), "test");
            var result = await controller.ConfirmEmail(request);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task Login_ValidRequest_ReturnsOkResult()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();

            var controller = CreateController(mockService);

            //Act
            var request = new LoginRequest { Email = "test", Password = "test" };
            var result = await controller.Login(request);

            //Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Login_NotFound_Returns400Result()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new AuthException("Test error", StatusCodes.Status400BadRequest, ApiErrorCodes.INVALID_CREDENTIALS));

            var controller = CreateController(mockService);

            //Act
            var request = new LoginRequest { Email = "test", Password = "test" };
            var result = await controller.Login(request);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(400);
            result.Should().BeOfType<ObjectResult>()
                .Which.Value.Should().BeOfType<ApiErrorResponse>()
                .Which.ErrorCode.Should().Be(ApiErrorCodes.INVALID_CREDENTIALS.ToString());
        }

        [Fact]
        public async Task Login_InternalError_Returns500Result()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test error"));

            var controller = CreateController(mockService);

            //Act
            var request = new LoginRequest { Email = "test", Password = "test" };
            var result = await controller.Login(request);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task ForgotPassword_ValidRequest_ReturnsOkResult()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();

            var controller = CreateController(mockService);

            //Act
            var request = new ForgotPasswordRequest { Email = "test" };
            var result = await controller.ForgotPassword(request);

            //Assert
            result.Should().BeOfType<OkResult>();
        }

        [Fact]
        public async Task ForgotPassword_InternalError_Returns500Result()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.ForgotPasswordAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test error"));

            var controller = CreateController(mockService);

            //Act
            var request = new ForgotPasswordRequest { Email = "test" };
            var result = await controller.ForgotPassword(request);

            //Assert
            result.Should().BeOfType<OkResult>();
        }

        [Fact]
        public async Task ResetPassword_ValidRequest_ReturnsOkResult()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();

            var controller = CreateController(mockService);

            //Act
            var request = new ResetPasswordRequest { Email = "test", ResetCode = "test", NewPassword = "test" };
            var result = await controller.ResetPassword(request);

            //Assert
            result.Should().BeOfType<OkResult>();
        }

        [Fact]
        public async Task ResetPassword_NotFound_Returns400Result()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.ResetPasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new AuthException("Test error", StatusCodes.Status400BadRequest, ApiErrorCodes.TOKEN_INVALID_OR_EXPIRED));

            var controller = CreateController(mockService);

            //Act
            var request = new ResetPasswordRequest { Email = "test", ResetCode = "test", NewPassword = "test" };
            var result = await controller.ResetPassword(request);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(400);
            result.Should().BeOfType<ObjectResult>()
                .Which.Value.Should().BeOfType<ApiErrorResponse>()
                .Which.ErrorCode.Should().Be(ApiErrorCodes.TOKEN_INVALID_OR_EXPIRED.ToString());
        }

        [Fact]
        public async Task ResetPassword_InternalError_Returns500Result()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.ResetPasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test error"));

            var controller = CreateController(mockService);

            //Act
            var request = new ResetPasswordRequest { Email = "test", ResetCode = "test", NewPassword = "test" };
            var result = await controller.ResetPassword(request);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task ChangePassword_ValidRequest_ReturnsOkResult()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var request = new ChangePasswordRequest("test", "test");
            var result = await controller.ChangePassword(request);

            //Assert
            result.Should().BeOfType<OkResult>();
        }

        [Fact]
        public async Task ChangePassword_NotFound_Returns400Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.ChangePasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new AuthException("Test error", StatusCodes.Status400BadRequest, ApiErrorCodes.AUTH_NO_LONGER_VALID));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var request = new ChangePasswordRequest("test", "test");
            var result = await controller.ChangePassword(request);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(400);
            result.Should().BeOfType<ObjectResult>()
                .Which.Value.Should().BeOfType<ApiErrorResponse>()
                .Which.ErrorCode.Should().Be(ApiErrorCodes.AUTH_NO_LONGER_VALID.ToString());
        }

        [Fact]
        public async Task ChangePassword_InternalError_Returns500Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.ChangePasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test error"));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var request = new ChangePasswordRequest("test", "test");
            var result = await controller.ChangePassword(request);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task ChangeEmail_ValidRequest_ReturnsOkResult()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var request = new ChangeEmailRequest("test", "test");
            var result = await controller.ChangeEmail(request);

            //Assert
            result.Should().BeOfType<OkResult>();
        }

        [Fact]
        public async Task ChangeEmail_NotFound_Returns400Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.ChangeEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new AuthException("Test error", StatusCodes.Status400BadRequest, ApiErrorCodes.AUTH_NO_LONGER_VALID));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var request = new ChangeEmailRequest("test", "test");
            var result = await controller.ChangeEmail(request);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(400);
            result.Should().BeOfType<ObjectResult>()
                .Which.Value.Should().BeOfType<ApiErrorResponse>()
                .Which.ErrorCode.Should().Be(ApiErrorCodes.AUTH_NO_LONGER_VALID.ToString());
        }

        [Fact]
        public async Task ChangeEmail_InternalError_Returns500Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.ChangeEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test error"));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var request = new ChangeEmailRequest("test", "test");
            var result = await controller.ChangeEmail(request);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task ChangeEmailConfirmation_ValidRequest_ReturnsOkResult()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            var controller = CreateController(mockService);

            //Act
            var request = new ChangeEmailConfirmationRequest(userId.ToString(), "test", "test");
            var result = await controller.ChangeEmailConfirmation(request);

            //Assert
            result.Should().BeOfType<OkResult>();
        }

        [Fact]
        public async Task ChangeEmailConfirmation_NotFound_Returns400Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.ChangeEmailConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new AuthException("Test error", StatusCodes.Status400BadRequest, ApiErrorCodes.AUTH_NO_LONGER_VALID));

            var controller = CreateController(mockService);

            //Act
            var request = new ChangeEmailConfirmationRequest(userId.ToString(), "test", "test");
            var result = await controller.ChangeEmailConfirmation(request);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(400);
            result.Should().BeOfType<ObjectResult>()
                .Which.Value.Should().BeOfType<ApiErrorResponse>()
                .Which.ErrorCode.Should().Be(ApiErrorCodes.AUTH_NO_LONGER_VALID.ToString());
        }

        [Fact]
        public async Task ChangeEmailConfirmation_InternalError_Returns500Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.ChangeEmailConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test error"));

            var controller = CreateController(mockService);

            //Act
            var request = new ChangeEmailConfirmationRequest(userId.ToString(), "test", "test");
            var result = await controller.ChangeEmailConfirmation(request);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task RefreshToken_ValidRequest_ReturnsOkResult()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            var controller = CreateController(mockService);
            controller.ControllerContext.HttpContext.Request.Headers["Cookies"] = "refresh_token=test_valid_token";

            //Act
            var result = await controller.RefreshToken();

            //Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task RefreshToken_NotFound_Returns400Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.RefreshTokenAsync(It.IsAny<string>()))
                .ThrowsAsync(new AuthException("Test error", StatusCodes.Status400BadRequest, ApiErrorCodes.AUTH_NO_LONGER_VALID));

            var controller = CreateController(mockService);
            controller.ControllerContext.HttpContext.Request.Headers["Cookies"] = "refresh_token=test_invalid_token";

            //Act
            var result = await controller.RefreshToken();

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(400);
            result.Should().BeOfType<ObjectResult>()
                .Which.Value.Should().BeOfType<ApiErrorResponse>()
                .Which.ErrorCode.Should().Be(ApiErrorCodes.AUTH_NO_LONGER_VALID.ToString());
        }

        [Fact]
        public async Task RefreshToken_InternalError_Returns500Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.RefreshTokenAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test error"));

            var controller = CreateController(mockService);

            //Act
            var result = await controller.RefreshToken();

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task Logout_ValidRequest_ReturnsOkResult()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var result = await controller.Logout();

            //Assert
            result.Should().BeOfType<OkResult>();
        }

        [Fact]
        public async Task Logout_InternalError_Returns500Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.LogoutAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test error"));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var result = await controller.Logout();

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task DeleteAccount_ValidRequest_ReturnsOkResult()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var request = new DeleteAccountRequest("test");
            var result = await controller.DeleteAccount(request);

            //Assert
            result.Should().BeOfType<OkResult>();
        }

        [Fact]
        public async Task DeleteAccount_NotFound_Returns400Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.DeleteAccountAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new AuthException("Test error", StatusCodes.Status400BadRequest, ApiErrorCodes.AUTH_NO_LONGER_VALID));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var request = new DeleteAccountRequest("test");
            var result = await controller.DeleteAccount(request);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(400);
            result.Should().BeOfType<ObjectResult>()
                .Which.Value.Should().BeOfType<ApiErrorResponse>()
                .Which.ErrorCode.Should().Be(ApiErrorCodes.AUTH_NO_LONGER_VALID.ToString());
        }

        [Fact]
        public async Task DeleteAccount_InternalError_Returns500Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.DeleteAccountAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test error"));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var request = new DeleteAccountRequest("test");
            var result = await controller.DeleteAccount(request);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

    }
}
