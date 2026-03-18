using FeedApp3.Api.Data.Repositories;
using FeedApp3.Api.Models;
using FeedApp3.Api.Services.Background;
using FeedApp3.Api.Services.External;
using FeedApp3.Api.Settings;
using FeedApp3.Shared.Services.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace FeedApp3.Api.Tests.Services
{
    public class FeedUpdateServiceTests
    {
        private IServiceScopeFactory CreateScopeFactory(IServiceProvider provider)
        {
            var scope = new Mock<IServiceScope>();
            scope.Setup(s => s.ServiceProvider).Returns(provider);

            var scopeFactory = new Mock<IServiceScopeFactory>();
            scopeFactory.Setup(x => x.CreateScope()).Returns(scope.Object);

            return scopeFactory.Object;
        }

        [Fact]
        public async Task ExecuteAsync_WhenFeedUpdatesExist_CompletesSuccessfully()
        {
            // Arrange
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();
            var feedId = Guid.NewGuid();

            var existingArticleUrl = "http://example.com/article1";

            var feedUpdates = new List<FeedUpdate>
            {
                new FeedUpdate {
                    FeedUpdateId = Guid.NewGuid(),
                    UserId = userId1,
                    RequestedAt = DateTime.UtcNow
                },
                new FeedUpdate {
                    FeedUpdateId = Guid.NewGuid(),
                    UserId = userId2,
                    RequestedAt = DateTime.UtcNow
                }
            };

            var existingFeedSummaries = new List<Feed>
            {
                new Feed { FeedId = feedId, UserId = userId1, LastChecked = DateTime.UtcNow.AddDays(-5) },
                new Feed { FeedId = Guid.NewGuid(), UserId = userId1, LastChecked = DateTime.UtcNow }
            };

            var existingFeed1 = new Feed
            {
                FeedId = feedId,
                UserId = userId1,
                FeedUrl = "http://example.com/feed1",
                Articles = new List<Article>() {
                    new Article
                    {
                        ArticleTitle = "Test Article 1",
                        ArticleContent = "Test Article Description 1",
                        ArticleUrl = existingArticleUrl,
                        ArticleDate = DateTime.UtcNow
                    }
                },
                LastModified = DateTime.UtcNow.AddDays(-7),
                LastChecked = DateTime.UtcNow.AddDays(-5)
            };
            var existingFeed2 = new Feed
            {
                FeedId = Guid.NewGuid(),
                UserId = userId2,
                FeedUrl = "http://example.com/feed2",
                Articles = new List<Article>(),
                LastModified = DateTime.UtcNow.AddDays(-7),
                LastChecked = DateTime.UtcNow.AddDays(-5)
            };

            var feedRepoMock = new Mock<IFeedRepository>();
            feedRepoMock.Setup(x => x.GetPendingFeedUpdatesAsync(It.IsAny<int>()))
                .ReturnsAsync(feedUpdates);
            feedRepoMock.Setup(x => x.GetListByUserIdAsync(userId1))
                .ReturnsAsync(existingFeedSummaries);
            feedRepoMock.Setup(x => x.GetListByUserIdAsync(userId2))
                .ReturnsAsync(new List<Feed>());
            feedRepoMock.Setup(x => x.GetByFeedIdAsync(userId1, It.IsAny<Guid>()))
                .ReturnsAsync(existingFeed1);
            feedRepoMock.Setup(x => x.GetByFeedIdAsync(userId2, It.IsAny<Guid>()))
                .ReturnsAsync((Feed?)null);
            var articlesToAdd = new List<Article>();
            feedRepoMock.Setup(x => x.CreateArticlesAsync(It.IsAny<List<Article>>()))
                .Callback<List<Article>>(a => articlesToAdd.AddRange(a))
                .Returns(Task.CompletedTask);
            feedRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Feed>()))
                .Returns(Task.CompletedTask);
            var feedUpdateUserIdsToDelete = new List<Guid>();
            feedRepoMock.Setup(x => x.DeleteFeedUpdatesAsync(It.IsAny<List<Guid>>()))
                .Callback<List<Guid>>(ids => feedUpdateUserIdsToDelete.AddRange(ids))
                .Returns(Task.CompletedTask);

            var importedFeedDto = new FeedDto
            {
                Articles = new List<ArticleDto>
                {
                    new ArticleDto
                    {
                        ArticleId = Guid.NewGuid(),
                        ArticleTitle = "Test Article 1",
                        ArticleContent = "Test Article Description 1",
                        ArticleUrl = existingArticleUrl,
                        ArticleDate = DateTime.UtcNow
                    },
                    new ArticleDto
                    {
                        ArticleId = Guid.NewGuid(),
                        ArticleTitle = "Test Article 2",
                        ArticleContent = "Test Article Description 2",
                        ArticleUrl = "http://example.com/article2",
                        ArticleDate = DateTime.UtcNow.AddDays(-1)
                    },
                    new ArticleDto
                    {
                        ArticleId = Guid.NewGuid(),
                        ArticleTitle = "Test Article 3",
                        ArticleContent = "Test Article Description 3",
                        ArticleUrl = "http://example.com/article3",
                        ArticleDate = DateTime.UtcNow.AddDays(-2)
                    }
                },
                LastModified = DateTime.UtcNow
            };

            var mockRssClient = new Mock<IRssClient>();
            mockRssClient.Setup(r => r.ImportFeedFromUrl(It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(importedFeedDto);

            var mockScopeProvider = new Mock<IServiceProvider>();
            mockScopeProvider.Setup(x => x.GetService(typeof(IFeedRepository)))
                .Returns(feedRepoMock.Object);
            mockScopeProvider.Setup(x => x.GetService(typeof(IRssClient)))
                .Returns(mockRssClient.Object);

            var mockScopeFactory = CreateScopeFactory(mockScopeProvider.Object);

            var applicationSettings = Options.Create(new ApplicationSettings
            {
                FeedUpdateBatchSize = 5,
                FeedUpdateMaxParallelism = 5,
                MinimumActiveUserRefreshIntervalInMinutes = 5
            });
            
            var service = new FeedUpdateService(mockScopeFactory, Mock.Of<ILogger<FeedUpdateService>>(), applicationSettings);

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(10000);

            // Act
            await service.StartAsync(cts.Token);
            await Task.Delay(50);

            // Assert
            mockRssClient.Verify(
                x => x.ImportFeedFromUrl(It.IsAny<string>(), It.IsAny<DateTime>()),
                Times.Once);

            articlesToAdd.Count().Should().Be(2);
            articlesToAdd.Should().NotContain(a => a.ArticleUrl == existingArticleUrl);

            feedUpdateUserIdsToDelete.Count().Should().Be(2);
            feedUpdateUserIdsToDelete.Should().Contain(new List<Guid>() { userId1, userId2 });
        }

        [Fact]
        public async Task ExecuteAsync_WhenNoFeedUpdates_DoesNothing()
        {
            // Arrange
            var feedUpdates = new List<FeedUpdate>();

            var feedRepoMock = new Mock<IFeedRepository>();
            feedRepoMock.Setup(x => x.GetPendingFeedUpdatesAsync(It.IsAny<int>()))
                .ReturnsAsync(feedUpdates);

            var mockScopeProvider = new Mock<IServiceProvider>();
            mockScopeProvider.Setup(x => x.GetService(typeof(IFeedRepository)))
                .Returns(feedRepoMock.Object);

            var mockScopeFactory = CreateScopeFactory(mockScopeProvider.Object);

            var applicationSettings = Options.Create(new ApplicationSettings
            {
                FeedUpdateBatchSize = 5
            });

            var service = new FeedUpdateService(mockScopeFactory, Mock.Of<ILogger<FeedUpdateService>>(), applicationSettings);

            // Act
            Func<Task> result = () => service.StartAsync(new CancellationToken());
            await Task.Delay(50);

            // Assert
            await result.Should().NotThrowAsync<Exception>();
            feedRepoMock.Verify(
                x => x.DeleteFeedUpdatesAsync(Mock.Of<List<Guid>>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_WhenDatabaseIsUnreachable_DoesNothing()
        {
            // Arrange
            var feedUpdates = new List<FeedUpdate>();

            var feedRepoMock = new Mock<IFeedRepository>();
            feedRepoMock.Setup(x => x.GetPendingFeedUpdatesAsync(It.IsAny<int>()))
                .ThrowsAsync(new Exception("test"));

            var mockScopeProvider = new Mock<IServiceProvider>();
            mockScopeProvider.Setup(x => x.GetService(typeof(IFeedRepository)))
                .Returns(feedRepoMock.Object);

            var mockScopeFactory = CreateScopeFactory(mockScopeProvider.Object);

            var applicationSettings = Options.Create(new ApplicationSettings
            {
                FeedUpdateBatchSize = 5
            });

            var service = new FeedUpdateService(mockScopeFactory, Mock.Of<ILogger<FeedUpdateService>>(), applicationSettings);

            // Act
            Func<Task> result = () => service.StartAsync(new CancellationToken());
            await Task.Delay(50);

            // Assert
            await result.Should().NotThrowAsync<Exception>();
            feedRepoMock.Verify(
                x => x.DeleteFeedUpdatesAsync(Mock.Of<List<Guid>>()),
                Times.Never);
        }

    }
}