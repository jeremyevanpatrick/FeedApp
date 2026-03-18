using FeedApp3.Api.Data.Repositories;
using FeedApp3.Api.Exceptions;
using FeedApp3.Api.Models;
using FeedApp3.Api.Services.Application;
using FeedApp3.Api.Services.External;
using FeedApp3.Shared.Services.DTOs;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FeedApp3.Api.Tests.Services
{
    public class FeedAppServiceTests
    {
        [Fact]
        public async Task GetFeedListAsync_WhenUserHasFeeds_ReturnsFeedList()
        {
            // Arrange
            var mockRepo = new Mock<IFeedRepository>();

            var feeds = new List<Feed>
            {
                new Feed
                {
                    Articles = new List<Article>
                    {
                        new Article { IsUnread = true },
                        new Article { IsUnread = true },
                        new Article { IsUnread = false }
                    }
                },
                new Feed
                {
                    Articles = new List<Article>
                    {
                        new Article { IsUnread = true },
                        new Article { IsUnread = false }
                    }
                }
            };

            mockRepo.Setup(r => r.GetListByUserIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync(feeds);

            var service = new FeedAppService(mockRepo.Object, Mock.Of<IRssClient>());

            // Act
            var result = await service.GetFeedListAsync(Guid.NewGuid());

            // Assert
            result.Should().HaveCount(2);
            result.Should().BeInDescendingOrder(f => f.UnreadArticleCount);

            mockRepo.Verify(
                r => r.CreateFeedUpdateAsync(It.IsAny<FeedUpdate>()),
                Times.Once);
        }

        [Fact]
        public async Task GetFeedListAsync_WhenNoFeedsFound_ReturnsEmptyList()
        {
            // Arrange
            var mockRepo = new Mock<IFeedRepository>();

            mockRepo.Setup(r => r.GetListByUserIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync(new List<Feed>());

            var service = new FeedAppService(mockRepo.Object, Mock.Of<IRssClient>());

            // Act
            var result = await service.GetFeedListAsync(Guid.NewGuid());

            // Assert
            result.Should().BeEmpty();

        }

        [Fact]
        public async Task GetFeedByIdAsync_WhenUserHasFeed_ReturnsFeed()
        {
            // Arrange
            var mockRepo = new Mock<IFeedRepository>();

            var feed = new Feed
            {
                Articles = new List<Article>
                {
                    new Article {
                        IsUnread = true,
                        ArticleDate = DateTime.UtcNow
                    },
                    new Article {
                        IsUnread = true,
                        ArticleDate = DateTime.UtcNow.AddMinutes(-60)
                    },
                    new Article {
                        IsUnread = false,
                        ArticleDate = DateTime.UtcNow.AddMinutes(-10)
                    }
                }
            };

            mockRepo.Setup(r => r.GetByFeedIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                    .ReturnsAsync(feed);

            var service = new FeedAppService(mockRepo.Object, Mock.Of<IRssClient>());

            // Act
            var result = await service.GetFeedByIdAsync(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            result.UnreadArticleCount.Should().Be(2);
            result.Articles.Should().BeInDescendingOrder(a => a.ArticleDate);

        }

        [Fact]
        public async Task GetFeedByIdAsync_WhenFeedNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var mockRepo = new Mock<IFeedRepository>();

            mockRepo.Setup(r => r.GetByFeedIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                    .ReturnsAsync((Feed?)null);

            var service = new FeedAppService(mockRepo.Object, Mock.Of<IRssClient>());

            // Act
            Func<Task> result = () => service.GetFeedByIdAsync(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            await result.Should().ThrowAsync<NotFoundException>();

        }

        [Fact]
        public async Task CreateAsync_WhenFeedIsValid_CreatesFeed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var feedUrl = "https://example.com/rss";

            var feedDto = new FeedDto
            {
                FeedId = Guid.NewGuid(),
                UserId = Guid.Empty,
                FeedUrl = feedUrl,
                Articles = new List<ArticleDto>
                {
                    new ArticleDto(),
                    new ArticleDto()
                }
            };

            var mockRepo = new Mock<IFeedRepository>();
            mockRepo.Setup(r => r.IsDuplicateFeedUrlAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(false);
            Feed? createdFeed = null;
            mockRepo.Setup(r => r.CreateAsync(It.IsAny<Feed>()))
                .Callback<Feed>(f => createdFeed = f)
                .Returns(Task.CompletedTask);

            var mockRssClient = new Mock<IRssClient>();
            mockRssClient.Setup(r => r.ImportFeedFromUrl(It.IsAny<string>()))
                    .ReturnsAsync(feedDto);

            var service = new FeedAppService(mockRepo.Object, mockRssClient.Object);

            // Act
            await service.CreateAsync(userId, feedUrl);

            // Assert
            createdFeed.Should().NotBeNull();
            createdFeed!.UserId.Should().Be(userId);
            createdFeed.FeedUrl.Should().Be(feedUrl);

        }

        [Fact]
        public async Task CreateAsync_WhenUrlIsDuplicate_DoesNothing()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var feedUrl = "https://example.com/rss";

            var mockRepo = new Mock<IFeedRepository>();
            mockRepo.Setup(r => r.IsDuplicateFeedUrlAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            var mockRssClient = new Mock<IRssClient>();

            var service = new FeedAppService(mockRepo.Object, mockRssClient.Object);

            // Act
            await service.CreateAsync(userId, feedUrl);

            // Assert
            mockRssClient.Verify(r => r.ImportFeedFromUrl(It.IsAny<string>()), Times.Never);

        }

        [Fact]
        public async Task CreateAsync_WhenFeedIsInvalid_ThrowsNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var feedUrl = "https://invalidrss";

            var mockRepo = new Mock<IFeedRepository>();
            mockRepo.Setup(r => r.IsDuplicateFeedUrlAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            var mockRssClient = new Mock<IRssClient>();
            mockRssClient.Setup(r => r.ImportFeedFromUrl(It.IsAny<string>()))
                 .ThrowsAsync(new Exception("Test error"));

            var service = new FeedAppService(mockRepo.Object, mockRssClient.Object);

            // Act
            Func<Task> result = () => service.CreateAsync(userId, feedUrl);

            // Assert
            await result.Should().ThrowAsync<NotFoundException>();

        }

        [Fact]
        public async Task UpdateAsync_WhenFeedIsValid_CreatesFeed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var feedId = Guid.NewGuid();

            var existingArticleId = Guid.NewGuid();
            var duplicateUrl = "https://example.com/rss/article1";

            var existingFeed = new Feed
            {
                FeedId = feedId,
                UserId = userId,
                FeedUrl = "https://example.com/rss",
                LastModified = DateTime.UtcNow.AddDays(-1),
                Articles = new List<Article>
                {
                    new Article
                    {
                        ArticleId = existingArticleId,
                        ArticleUrl = duplicateUrl
                    }
                }
            };

            var latestFeed = new FeedDto
            {
                LastModified = DateTime.UtcNow.AddMinutes(-30),
                LastChecked = DateTime.UtcNow,
                Articles = new List<ArticleDto>
                {
                    new ArticleDto
                    {
                        ArticleId = existingArticleId,
                        ArticleUrl = duplicateUrl
                    },
                    new ArticleDto
                    {
                        ArticleId = Guid.NewGuid(),
                        ArticleUrl = "https://example.com/rss/article2"
                    },
                    new ArticleDto
                    {
                        ArticleId = Guid.NewGuid(),
                        ArticleUrl = "https://example.com/rss/article3"
                    }
                }
            };

            var mockRepo = new Mock<IFeedRepository>();

            mockRepo.Setup(r => r.GetByFeedIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                    .ReturnsAsync(existingFeed);

            Feed? updatedFeed = null;
            mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Feed>()))
                .Callback<Feed>(f => updatedFeed = f)
                .Returns(Task.CompletedTask);

            List<Article> addedArticles = new List<Article>();
            mockRepo.Setup(r => r.CreateArticlesAsync(It.IsAny<List<Article>>()))
                .Callback<List<Article>>(a => addedArticles = a)
                .Returns(Task.CompletedTask);

            var mockRssClient = new Mock<IRssClient>();
            mockRssClient.Setup(r => r.ImportFeedFromUrl(It.IsAny<string>(), It.IsAny<DateTime>()))
                    .ReturnsAsync(latestFeed);

            var service = new FeedAppService(mockRepo.Object, mockRssClient.Object);

            // Act
            await service.UpdateAsync(userId, feedId);

            // Assert
            updatedFeed.Should().NotBeNull();
            updatedFeed!.LastModified.Should().Be(latestFeed.LastModified);
            updatedFeed.LastChecked.Should().Be(latestFeed.LastChecked);
            updatedFeed.Articles.Count.Should().Be(1);
            updatedFeed.Articles.Should().Contain(a => a.ArticleUrl == duplicateUrl);
            addedArticles.Count.Should().Be(2);
            addedArticles.Should().NotContain(a => a.ArticleUrl == duplicateUrl);

        }

        [Fact]
        public async Task UpdateAsync_WhenFeedNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var feedId = Guid.NewGuid();

            var mockRepo = new Mock<IFeedRepository>();
            mockRepo.Setup(r => r.GetByFeedIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                    .ReturnsAsync((Feed?)null);

            var service = new FeedAppService(mockRepo.Object, Mock.Of<IRssClient>());

            // Act
            Func<Task> result = () => service.UpdateAsync(userId, feedId);

            // Assert
            await result.Should().ThrowAsync<NotFoundException>();

        }

        [Fact]
        public async Task UpdateAsync_WhenFeedIsInvalid_ThrowsException()
        {
            // Arrange
            var existingFeed = new Feed
            {
                FeedUrl = "https://nolongervalid",
                LastModified = DateTime.UtcNow.AddDays(-1)
            };

            var mockRepo = new Mock<IFeedRepository>();
            mockRepo.Setup(r => r.GetByFeedIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                 .ReturnsAsync(existingFeed);

            var mockRssClient = new Mock<IRssClient>();
            mockRssClient.Setup(r => r.ImportFeedFromUrl(It.IsAny<string>()))
                 .ThrowsAsync(new Exception("Test error"));

            var service = new FeedAppService(mockRepo.Object, mockRssClient.Object);

            // Act
            Func<Task> result = () => service.UpdateAsync(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            await result.Should().ThrowAsync<Exception>();

        }

        [Fact]
        public async Task DeleteAsync_WhenFeedExists_DeletesFeed()
        {
            // Arrange
            var mockRepo = new Mock<IFeedRepository>();

            var existingFeed = new Feed();

            mockRepo.Setup(r => r.GetByFeedIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(existingFeed);

            Feed? deletedFeed = null;
            mockRepo.Setup(r => r.DeleteAsync(It.IsAny<Feed>()))
                .Callback<Feed>(f => deletedFeed = f)
                .Returns(Task.CompletedTask);

            var service = new FeedAppService(mockRepo.Object, Mock.Of<IRssClient>());

            // Act
            await service.DeleteAsync(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            deletedFeed.Should().NotBeNull();

        }

        [Fact]
        public async Task DeleteAsync_WhenFeedNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var mockRepo = new Mock<IFeedRepository>();
            mockRepo.Setup(r => r.GetByFeedIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                    .ReturnsAsync((Feed?)null);

            var service = new FeedAppService(mockRepo.Object, Mock.Of<IRssClient>());

            // Act
            Func<Task> result = () => service.DeleteAsync(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            await result.Should().ThrowAsync<NotFoundException>();

        }

        [Fact]
        public async Task MarkFeedAsReadAsync_WhenFeedExists_MarksAllArticlesAsRead()
        {
            // Arrange
            var mockRepo = new Mock<IFeedRepository>();

            var existingFeed = new Feed
            {
                Articles = new List<Article>
                {
                    new Article { IsUnread = true },
                    new Article { IsUnread = true },
                    new Article { IsUnread = false }
                }
            };

            mockRepo.Setup(r => r.GetByFeedIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(existingFeed);

            Feed? updatedFeed = null;
            mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Feed>()))
                .Callback<Feed>(f => updatedFeed = f)
                .Returns(Task.CompletedTask);

            var service = new FeedAppService(mockRepo.Object, Mock.Of<IRssClient>());

            // Act
            await service.MarkFeedAsReadAsync(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            updatedFeed.Should().NotBeNull();
            updatedFeed.Articles.Count.Should().Be(3);
            updatedFeed.Articles.Should().NotContain(a => a.IsUnread);

        }

        [Fact]
        public async Task MarkFeedAsReadAsync_WhenFeedNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var mockRepo = new Mock<IFeedRepository>();
            mockRepo.Setup(r => r.GetByFeedIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync((Feed?)null);

            var service = new FeedAppService(mockRepo.Object, Mock.Of<IRssClient>());

            // Act
            Func<Task> result = () => service.MarkFeedAsReadAsync(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            await result.Should().ThrowAsync<NotFoundException>();

        }

        [Fact]
        public async Task MarkArticleAsReadAsync_WhenArticleExists_MarksArticleAsRead()
        {
            // Arrange
            var mockRepo = new Mock<IFeedRepository>();

            var existingArticle = new Article { IsUnread = true };

            mockRepo.Setup(r => r.GetByArticleIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(existingArticle);

            Article? updatedArticle = null;
            mockRepo.Setup(r => r.UpdateArticleAsync(It.IsAny<Article>()))
                .Callback<Article>(a => updatedArticle = a)
                .Returns(Task.CompletedTask);

            var service = new FeedAppService(mockRepo.Object, Mock.Of<IRssClient>());

            // Act
            await service.MarkArticleAsReadAsync(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            updatedArticle.Should().NotBeNull();
            updatedArticle.IsUnread.Should().Be(false);

        }

        [Fact]
        public async Task MarkArticleAsReadAsync_WhenFeedNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var mockRepo = new Mock<IFeedRepository>();
            mockRepo.Setup(r => r.GetByArticleIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync((Article?)null);

            var service = new FeedAppService(mockRepo.Object, Mock.Of<IRssClient>());

            // Act
            Func<Task> result = () => service.MarkArticleAsReadAsync(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            await result.Should().ThrowAsync<NotFoundException>();

        }
    }
}