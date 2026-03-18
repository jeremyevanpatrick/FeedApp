using FeedApp3.Api.Controllers;
using FeedApp3.Api.Exceptions;
using FeedApp3.Api.Services.Application;
using FeedApp3.Shared.Services.DTOs;
using FeedApp3.Shared.Services.Requests;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Helpers;
using System.Data.Common;
using System.Security.Claims;

namespace FeedApp3.Api.Tests.Controllers
{
    public class FeedsControllerTests
    {
        private static FeedsController CreateControllerWithUser(Guid userId, Mock<IFeedAppService> mockService)
        {
            var controller = new FeedsController(
                Mock.Of<ILogger<FeedsController>>(),
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
        public async Task GetFeedList_ValidRequest_ReturnsOkResult()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IFeedAppService>();

            mockService
                .Setup(s => s.GetFeedListAsync(userId))
                .ReturnsAsync(new List<FeedDto>());

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var result = await controller.GetFeedList();

            //Assert
            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetFeedList_InternalError_Returns500Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IFeedAppService>();

            mockService
                .Setup(s => s.GetFeedListAsync(It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("Test error"));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var result = await controller.GetFeedList();

            //Assert
            result.Result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task GetFeedById_ValidRequest_ReturnsOkResult()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IFeedAppService>();

            mockService
                .Setup(s => s.GetFeedByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(new FeedDto());

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var result = await controller.GetFeedById(Guid.NewGuid());

            //Assert
            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetFeedById_NotFound_Returns400Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IFeedAppService>();

            mockService
                .Setup(s => s.GetFeedByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ThrowsAsync(new NotFoundException("Test error"));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var result = await controller.GetFeedById(Guid.NewGuid());

            //Assert
            result.Result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(400);
            result.Result.Should().BeOfType<ObjectResult>()
                .Which.Value.Should().BeOfType<ProblemDetails>()
                .Which.Extensions["errorCode"].Should().Be(ResponseErrorCodes.INVALID_REQUEST_PARAMETERS.ToString());
        }

        [Fact]
        public async Task GetFeedById_InternalError_Returns500Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IFeedAppService>();

            mockService
                .Setup(s => s.GetFeedByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("Test error"));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var result = await controller.GetFeedById(Guid.NewGuid());

            //Assert
            result.Result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task Create_ValidRequest_ReturnsOkResult()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IFeedAppService>();

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var request = new CreateFeedRequest("http://example.com/feed");
            var result = await controller.Create(request);

            //Assert
            result.Should().BeOfType<OkResult>();
        }

        [Fact]
        public async Task Create_NotFound_Returns400Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IFeedAppService>();

            mockService
                .Setup(s => s.CreateAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ThrowsAsync(new NotFoundException("Test error"));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var request = new CreateFeedRequest("http://example.com/feed");
            var result = await controller.Create(request);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(400);
            result.Should().BeOfType<ObjectResult>()
                .Which.Value.Should().BeOfType<ProblemDetails>()
                .Which.Extensions["errorCode"].Should().Be(ResponseErrorCodes.INVALID_REQUEST_PARAMETERS.ToString());
        }

        [Fact]
        public async Task Create_InternalError_Returns500Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IFeedAppService>();

            mockService
                .Setup(s => s.CreateAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test error"));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var request = new CreateFeedRequest("http://example.com/feed");
            var result = await controller.Create(request);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task Update_ValidRequest_ReturnsOkResult()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var feedId = Guid.NewGuid();
            var mockService = new Mock<IFeedAppService>();

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var request = new UpdateFeedRequest(feedId);
            var result = await controller.Update(request);

            //Assert
            result.Should().BeOfType<OkResult>();
        }

        [Fact]
        public async Task Update_NotFound_Returns400Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var feedId = Guid.NewGuid();
            var mockService = new Mock<IFeedAppService>();

            mockService
                .Setup(s => s.UpdateAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ThrowsAsync(new NotFoundException("Test error"));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var request = new UpdateFeedRequest(feedId);
            var result = await controller.Update(request);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(400);
            result.Should().BeOfType<ObjectResult>()
                .Which.Value.Should().BeOfType<ProblemDetails>()
                .Which.Extensions["errorCode"].Should().Be(ResponseErrorCodes.INVALID_REQUEST_PARAMETERS.ToString());
        }

        [Fact]
        public async Task Update_InternalError_Returns500Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var feedId = Guid.NewGuid();
            var mockService = new Mock<IFeedAppService>();

            mockService
                .Setup(s => s.UpdateAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("Test error"));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var request = new UpdateFeedRequest(feedId);
            var result = await controller.Update(request);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task Delete_ValidRequest_ReturnsOkResult()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var feedId = Guid.NewGuid();
            var mockService = new Mock<IFeedAppService>();

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var request = new DeleteFeedRequest(feedId);
            var result = await controller.Delete(request);

            //Assert
            result.Should().BeOfType<OkResult>();
        }

        [Fact]
        public async Task Delete_NotFound_Returns400Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var feedId = Guid.NewGuid();
            var mockService = new Mock<IFeedAppService>();

            mockService
                .Setup(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ThrowsAsync(new NotFoundException("Test error"));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var request = new DeleteFeedRequest(feedId);
            var result = await controller.Delete(request);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(400);
            result.Should().BeOfType<ObjectResult>()
                .Which.Value.Should().BeOfType<ProblemDetails>()
                .Which.Extensions["errorCode"].Should().Be(ResponseErrorCodes.INVALID_REQUEST_PARAMETERS.ToString());
        }

        [Fact]
        public async Task Delete_InternalError_Returns500Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var feedId = Guid.NewGuid();
            var mockService = new Mock<IFeedAppService>();

            mockService
                .Setup(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("Test error"));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var request = new DeleteFeedRequest(feedId);
            var result = await controller.Delete(request);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task MarkFeedAsRead_ValidRequest_ReturnsOkResult()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var feedId = Guid.NewGuid();
            var mockService = new Mock<IFeedAppService>();

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var request = new MarkFeedAsReadRequest(feedId);
            var result = await controller.MarkFeedAsRead(request);

            //Assert
            result.Should().BeOfType<OkResult>();
        }

        [Fact]
        public async Task MarkFeedAsRead_NotFound_Returns400Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var feedId = Guid.NewGuid();
            var mockService = new Mock<IFeedAppService>();

            mockService
                .Setup(s => s.MarkFeedAsReadAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ThrowsAsync(new NotFoundException("Test error"));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var request = new MarkFeedAsReadRequest(feedId);
            var result = await controller.MarkFeedAsRead(request);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(400);
            result.Should().BeOfType<ObjectResult>()
                .Which.Value.Should().BeOfType<ProblemDetails>()
                .Which.Extensions["errorCode"].Should().Be(ResponseErrorCodes.INVALID_REQUEST_PARAMETERS.ToString());
        }

        [Fact]
        public async Task MarkFeedAsRead_InternalError_Returns500Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var feedId = Guid.NewGuid();
            var mockService = new Mock<IFeedAppService>();

            mockService
                .Setup(s => s.MarkFeedAsReadAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("Test error"));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var request = new MarkFeedAsReadRequest(feedId);
            var result = await controller.MarkFeedAsRead(request);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task MarkArticleAsRead_ValidRequest_ReturnsOkResult()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var feedId = Guid.NewGuid();
            var mockService = new Mock<IFeedAppService>();

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var request = new MarkArticleAsReadRequest(feedId);
            var result = await controller.MarkArticleAsRead(request);

            //Assert
            result.Should().BeOfType<OkResult>();
        }

        [Fact]
        public async Task MarkArticleAsRead_NotFound_Returns400Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var feedId = Guid.NewGuid();
            var mockService = new Mock<IFeedAppService>();

            mockService
                .Setup(s => s.MarkArticleAsReadAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ThrowsAsync(new NotFoundException("Test error"));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var request = new MarkArticleAsReadRequest(feedId);
            var result = await controller.MarkArticleAsRead(request);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(400);
            result.Should().BeOfType<ObjectResult>()
                .Which.Value.Should().BeOfType<ProblemDetails>()
                .Which.Extensions["errorCode"].Should().Be(ResponseErrorCodes.INVALID_REQUEST_PARAMETERS.ToString());
        }

        [Fact]
        public async Task MarkArticleAsRead_InternalError_Returns500Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var feedId = Guid.NewGuid();
            var mockService = new Mock<IFeedAppService>();

            mockService
                .Setup(s => s.MarkArticleAsReadAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("Test error"));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var request = new MarkArticleAsReadRequest(feedId);
            var result = await controller.MarkArticleAsRead(request);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }
    }
}
