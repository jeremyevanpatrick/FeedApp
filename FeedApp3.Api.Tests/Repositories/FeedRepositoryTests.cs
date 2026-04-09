using FeedApp3.Api.Data.Context;
using FeedApp3.Api.Data.Repositories;
using FeedApp3.Api.Models;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace FeedApp3.Api.Tests.Repositories
{
    public class FeedRepositoryTests
    {
        private AppDbContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private AppDbContext CreateSqLiteDb()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public async Task GetListByUserIdAsync_WhenUserHasFeeds_ReturnsFeeds()
        {
            // Arrange
            var db = CreateDb();

            var userId = Guid.NewGuid();

            db.Feeds.Add(new Feed
            {
                FeedId = Guid.NewGuid(),
                UserId = userId,
                FeedTitle = "Test 1",
                FeedUrl = "https://example1.com/rss",
                BlogUrl = "https://example1.com",
            });

            db.Feeds.Add(new Feed
            {
                FeedId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                FeedTitle = "Test 2",
                FeedUrl = "https://example2.com/rss",
                BlogUrl = "https://example2.com",
            });

            db.Feeds.Add(new Feed
            {
                FeedId = Guid.NewGuid(),
                UserId = userId,
                FeedTitle = "Test 3",
                FeedUrl = "https://example3.com/rss",
                BlogUrl = "https://example3.com",
            });

            await db.SaveChangesAsync();

            var repo = new FeedRepository(Mock.Of<ILogger<FeedRepository>>(), db);

            // Act
            var result = await repo.GetListByUserIdAsync(userId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(f => f.UserId == userId);
        }

        [Fact]
        public async Task GetByFeedIdAsync_WhenFeedExists_ReturnsFeed()
        {
            // Arrange
            var db = CreateDb();

            var feedId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            db.Feeds.Add(new Feed
            {
                FeedId = Guid.NewGuid(),
                UserId = userId,
                FeedTitle = "Test 1",
                FeedUrl = "https://example1.com/rss",
                BlogUrl = "https://example1.com",
            });

            db.Feeds.Add(new Feed
            {
                FeedId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                FeedTitle = "Test 2",
                FeedUrl = "https://example2.com/rss",
                BlogUrl = "https://example2.com",
            });

            db.Feeds.Add(new Feed
            {
                FeedId = feedId,
                UserId = userId,
                FeedTitle = "Test 3",
                FeedUrl = "https://example3.com/rss",
                BlogUrl = "https://example3.com",
                Articles = new List<Article>
                {
                    new Article
                    {
                        ArticleId = Guid.NewGuid(),
                        ArticleTitle = "Article 1",
                        ArticleContent = "Content 1",
                        ArticleUrl = "https://example3.com/article1",
                        ArticleDate = DateTime.UtcNow.AddDays(-2)
                    },
                    new Article
                    {
                        ArticleId = Guid.NewGuid(),
                        ArticleTitle = "Article 2",
                        ArticleContent = "Content 2",
                        ArticleUrl = "https://example3.com/article2",
                        ArticleDate = DateTime.UtcNow.AddDays(-1)
                    },
                    new Article
                    {
                        ArticleId = Guid.NewGuid(),
                        ArticleTitle = "Article 3",
                        ArticleContent = "Content 3",
                        ArticleUrl = "https://example3.com/article3",
                        ArticleDate = DateTime.UtcNow
                    }
                }
            });

            await db.SaveChangesAsync();

            var repo = new FeedRepository(Mock.Of<ILogger<FeedRepository>>(), db);

            // Act
            var result = await repo.GetByFeedIdAsync(userId, feedId);

            // Assert
            result.Should().NotBeNull();
            result!.FeedId.Should().Be(feedId);
            result.Articles.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetByArticleIdAsync_WhenArticleExists_ReturnsArticle()
        {
            // Arrange
            var db = CreateDb();

            var articleId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var feedId = Guid.NewGuid();

            db.Feeds.Add(new Feed
            {
                FeedId = feedId,
                UserId = userId,
                FeedTitle = "Test 3",
                FeedUrl = "https://example3.com/rss",
                BlogUrl = "https://example3.com",
                Articles = new List<Article>
                {
                    new Article
                    {
                        FeedId = feedId,
                        ArticleId = Guid.NewGuid(),
                        ArticleTitle = "Article 1",
                        ArticleContent = "Content 1",
                        ArticleUrl = "https://example3.com/article1",
                        ArticleDate = DateTime.UtcNow.AddDays(-2)
                    },
                    new Article
                    {
                        FeedId = feedId,
                        ArticleId = articleId,
                        ArticleTitle = "Article 2",
                        ArticleContent = "Content 2",
                        ArticleUrl = "https://example3.com/article2",
                        ArticleDate = DateTime.UtcNow.AddDays(-1)
                    },
                    new Article
                    {
                        FeedId = feedId,
                        ArticleId = Guid.NewGuid(),
                        ArticleTitle = "Article 3",
                        ArticleContent = "Content 3",
                        ArticleUrl = "https://example3.com/article3",
                        ArticleDate = DateTime.UtcNow
                    }
                }
            });

            await db.SaveChangesAsync();

            var repo = new FeedRepository(Mock.Of<ILogger<FeedRepository>>(), db);

            // Act
            var result = await repo.GetByArticleIdAsync(userId, articleId);

            // Assert
            result.Should().NotBeNull();
            result!.ArticleId.Should().Be(articleId);
            result.Feed.UserId.Should().Be(userId);
        }

        [Fact]
        public async Task IsDuplicateFeedUrlAsync_WhenDuplicateExists_ReturnsTrue()
        {
            // Arrange
            var db = CreateDb();

            var feedUrl = "https://example3.com/rss";
            var userId = Guid.NewGuid();

            db.Feeds.Add(new Feed
            {
                FeedId = Guid.NewGuid(),
                UserId = userId,
                FeedTitle = "Test 3",
                FeedUrl = feedUrl,
                BlogUrl = "https://example3.com"
            });

            await db.SaveChangesAsync();

            var repo = new FeedRepository(Mock.Of<ILogger<FeedRepository>>(), db);

            // Act
            var result = await repo.IsDuplicateFeedUrlAsync(userId, feedUrl);

            // Assert
            result.Should().Be(true);
        }

        [Fact]
        public async Task IsDuplicateFeedUrlAsync_WhenUnique_ReturnsFalse()
        {
            // Arrange
            var db = CreateDb();

            var feedUrl = "https://unique.com/rss";
            var userId = Guid.NewGuid();

            db.Feeds.Add(new Feed
            {
                FeedId = Guid.NewGuid(),
                UserId = userId,
                FeedTitle = "Test 3",
                FeedUrl = "https://example3.com/rss",
                BlogUrl = "https://example3.com"
            });

            await db.SaveChangesAsync();

            var repo = new FeedRepository(Mock.Of<ILogger<FeedRepository>>(), db);

            // Act
            var result = await repo.IsDuplicateFeedUrlAsync(userId, feedUrl);

            // Assert
            result.Should().Be(false);
        }

        [Fact]
        public async Task CreateAsync_WhenUniqueFeed_CreatesFeed()
        {
            // Arrange
            var db = CreateDb();

            var articleId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var feedId = Guid.NewGuid();

            var feed = new Feed
            {
                FeedId = feedId,
                UserId = userId,
                FeedTitle = "Test 3",
                FeedUrl = "https://example3.com/rss",
                BlogUrl = "https://example3.com",
                Articles = new List<Article>
                {
                    new Article
                    {
                        FeedId = feedId,
                        ArticleId = Guid.NewGuid(),
                        ArticleTitle = "Article 1",
                        ArticleContent = "Content 1",
                        ArticleUrl = "https://example3.com/article1",
                        ArticleDate = DateTime.UtcNow.AddDays(-2)
                    },
                    new Article
                    {
                        FeedId = feedId,
                        ArticleId = articleId,
                        ArticleTitle = "Article 2",
                        ArticleContent = "Content 2",
                        ArticleUrl = "https://example3.com/article2",
                        ArticleDate = DateTime.UtcNow.AddDays(-1)
                    },
                    new Article
                    {
                        FeedId = feedId,
                        ArticleId = Guid.NewGuid(),
                        ArticleTitle = "Article 3",
                        ArticleContent = "Content 3",
                        ArticleUrl = "https://example3.com/article3",
                        ArticleDate = DateTime.UtcNow
                    }
                }
            };

            var repo = new FeedRepository(Mock.Of<ILogger<FeedRepository>>(), db);

            // Act
            await repo.CreateAsync(feed);

            // Assert
            var createdFeed = await db.Feeds
                .Include(f => f.Articles)
                .AsNoTracking()
                .FirstOrDefaultAsync();
            createdFeed.Should().NotBeNull();
            createdFeed!.FeedId.Should().Be(feedId);
            createdFeed.UserId.Should().Be(userId);
            createdFeed.Articles.Count.Should().Be(3);
        }

        [Fact]
        public async Task CreateAsync_WhenDuplicateFeed_ThrowsException()
        {
            // Arrange
            var db = CreateDb();

            var articleId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var feedId = Guid.NewGuid();

            var feed = new Feed
            {
                FeedId = feedId,
                UserId = userId,
                FeedTitle = "Test 3",
                FeedUrl = "https://example3.com/rss",
                BlogUrl = "https://example3.com"
            };

            db.Feeds.Add(feed);

            await db.SaveChangesAsync();

            var duplicateFeed = new Feed
            {
                FeedId = feedId,
                UserId = userId,
                FeedTitle = "Test 3",
                FeedUrl = "https://example3.com/rss",
                BlogUrl = "https://example3.com"
            };

            var repo = new FeedRepository(Mock.Of<ILogger<FeedRepository>>(), db);

            // Act
            Func<Task> result = () => repo.CreateAsync(duplicateFeed);

            // Assert
            await result.Should().ThrowAsync<Exception>();

        }

        [Fact]
        public async Task UpdateAsync_WhenFeedExists_UpdatesFeed()
        {
            // Arrange
            var db = CreateDb();

            var existingArticleId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var feedId = Guid.NewGuid();

            var feed = new Feed
            {
                FeedId = feedId,
                UserId = userId,
                FeedTitle = "Test 3",
                FeedUrl = "https://example3.com/rss",
                BlogUrl = "https://example3.com",
                Articles = new List<Article>
                {
                    new Article
                    {
                        FeedId = feedId,
                        ArticleId = existingArticleId,
                        ArticleTitle = "Article 1",
                        ArticleContent = "Content 1",
                        ArticleUrl = "https://example3.com/article1",
                        ArticleDate = DateTime.UtcNow.AddDays(-2),
                        IsUnread = true
                    }
                },
                LastChecked = DateTime.UtcNow.AddDays(-2),
                LastModified = DateTime.UtcNow.AddDays(-2)
            };

            db.Feeds.Add(feed);

            await db.SaveChangesAsync();

            db.Entry(feed).State = EntityState.Detached;

            var repo = new FeedRepository(Mock.Of<ILogger<FeedRepository>>(), db);

            // Act
            var newLastChecked = DateTime.UtcNow;

            var feedToUpdate = await db.Feeds
                .Include(f => f.Articles)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            feedToUpdate!.LastChecked = newLastChecked;
            feedToUpdate.LastModified = DateTime.UtcNow;
            feedToUpdate.Articles.ForEach(a =>
            {
                if (a.ArticleId == existingArticleId)
                {
                    a.IsUnread = false;
                }
            });

            await repo.UpdateAsync(feedToUpdate);

            // Assert
            var updatedFeedResult = await db.Feeds
                .Include(f => f.Articles)
                .AsNoTracking()
                .FirstOrDefaultAsync();
            updatedFeedResult.Should().NotBeNull();
            updatedFeedResult!.FeedId.Should().Be(feedId);
            updatedFeedResult.UserId.Should().Be(userId);
            updatedFeedResult.Articles.Count.Should().Be(1);
            updatedFeedResult.Articles.Should().Contain(a=>a.ArticleId == existingArticleId);
            updatedFeedResult.LastChecked.Should().Be(newLastChecked);

        }

        [Fact]
        public async Task UpdateAsync_WhenFeedDoesNotExist_ThrowsException()
        {
            // Arrange
            var db = CreateDb();
            var repo = new FeedRepository(Mock.Of<ILogger<FeedRepository>>(), db);

            // Act
            var updatedFeed = new Feed
            {
                FeedId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                FeedTitle = "Test 3",
                FeedUrl = "https://example3.com/rss",
                BlogUrl = "https://example3.com"
            };
            Func<Task> result = () => repo.UpdateAsync(updatedFeed);

            // Assert
            await result.Should().ThrowAsync<Exception>();

        }

        [Fact]
        public async Task DeleteAsync_WhenFeedExists_DeletesFeed()
        {
            // Arrange
            var db = CreateDb();

            var feedId = Guid.NewGuid();

            var feed = new Feed
            {
                FeedId = feedId,
                UserId = Guid.NewGuid(),
                FeedTitle = "Test 3",
                FeedUrl = "https://example3.com/rss",
                BlogUrl = "https://example3.com",
                Articles = new List<Article>
                {
                    new Article
                    {
                        FeedId = feedId,
                        ArticleId = Guid.NewGuid(),
                        ArticleTitle = "Article 1",
                        ArticleContent = "Content 1",
                        ArticleUrl = "https://example3.com/article1",
                        ArticleDate = DateTime.UtcNow.AddDays(-2),
                        IsUnread = true
                    }
                }
            };

            db.Feeds.Add(feed);

            await db.SaveChangesAsync();

            db.Entry(feed).State = EntityState.Detached;

            var repo = new FeedRepository(Mock.Of<ILogger<FeedRepository>>(), db);

            // Act
            var feedToDelete = await db.Feeds
                .Include(f => f.Articles)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            await repo.DeleteAsync(feedToDelete);

            // Assert
            var deletedFeedResult = await db.Feeds
                .AsNoTracking()
                .FirstOrDefaultAsync();
            deletedFeedResult.Should().BeNull();

            var deletedArticleResult = await db.Articles
                .AsNoTracking()
                .FirstOrDefaultAsync();
            deletedArticleResult.Should().BeNull();

        }

        [Fact]
        public async Task DeleteAsync_WhenFeedDoesNotExist_ThrowsException()
        {
            // Arrange
            var db = CreateDb();
            var repo = new FeedRepository(Mock.Of<ILogger<FeedRepository>>(), db);

            // Act
            var feedToDelete = new Feed
            {
                FeedId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                FeedTitle = "Test 3",
                FeedUrl = "https://example3.com/rss",
                BlogUrl = "https://example3.com"
            };
            Func<Task> result = () => repo.DeleteAsync(feedToDelete);

            // Assert
            await result.Should().ThrowAsync<Exception>();

        }

        [Fact]
        public async Task CreateArticlesAsync_WhenUniqueArticles_CreatesArticles()
        {
            // Arrange
            var db = CreateDb();

            var feedId = Guid.NewGuid();

            var feed = new Feed
            {
                FeedId = feedId,
                UserId = Guid.NewGuid(),
                FeedTitle = "Test 1",
                FeedUrl = "https://example3.com/rss",
                BlogUrl = "https://example3.com",
                Articles = new List<Article> {
                    new Article
                    {
                        FeedId = feedId,
                        ArticleId = Guid.NewGuid(),
                        ArticleTitle = "Article 1",
                        ArticleContent = "Content 1",
                        ArticleUrl = "https://example3.com/article1",
                        ArticleDate = DateTime.UtcNow.AddDays(-2),
                        IsUnread = true
                    }
                }
            };

            db.Feeds.Add(feed);

            await db.SaveChangesAsync();

            var repo = new FeedRepository(Mock.Of<ILogger<FeedRepository>>(), db);

            // Act
            var articlesToCreate = new List<Article>
            {
                new Article
                {
                    FeedId = feedId,
                    ArticleId = Guid.NewGuid(),
                    ArticleTitle = "Article 2",
                    ArticleContent = "Content 2",
                    ArticleUrl = "https://example3.com/article2",
                    ArticleDate = DateTime.UtcNow.AddDays(-1),
                    IsUnread = true
                },
                new Article
                {
                    FeedId = feedId,
                    ArticleId = Guid.NewGuid(),
                    ArticleTitle = "Article 3",
                    ArticleContent = "Content 3",
                    ArticleUrl = "https://example3.com/article3",
                    ArticleDate = DateTime.UtcNow,
                    IsUnread = true
                }
            };

            await repo.CreateArticlesAsync(articlesToCreate);

            // Assert
            var updatedFeed = await db.Feeds
                .Include(f => f.Articles)
                .AsNoTracking()
                .FirstOrDefaultAsync();
            updatedFeed!.Articles.Count.Should().Be(3);

        }

        [Fact]
        public async Task CreateArticlesAsync_WhenDuplicateExists_ThrowException()
        {
            // Arrange
            var db = CreateDb();

            var feedId = Guid.NewGuid();
            var articleId = Guid.NewGuid();

            var feed = new Feed
            {
                FeedId = feedId,
                UserId = Guid.NewGuid(),
                FeedTitle = "Test 1",
                FeedUrl = "https://example3.com/rss",
                BlogUrl = "https://example3.com",
                Articles = new List<Article> {
                    new Article
                    {
                        FeedId = feedId,
                        ArticleId = articleId,
                        ArticleTitle = "Article 1",
                        ArticleContent = "Content 1",
                        ArticleUrl = "https://example3.com/article1",
                        ArticleDate = DateTime.UtcNow.AddDays(-2),
                        IsUnread = true
                    }
                }
            };

            db.Feeds.Add(feed);

            await db.SaveChangesAsync();

            var repo = new FeedRepository(Mock.Of<ILogger<FeedRepository>>(), db);

            // Act
            var articlesToCreate = new List<Article>
            {
                new Article
                {
                    FeedId = feedId,
                    ArticleId = articleId,
                    ArticleTitle = "Article 1",
                    ArticleContent = "Content 1",
                    ArticleUrl = "https://example3.com/article1",
                    ArticleDate = DateTime.UtcNow.AddDays(-2),
                    IsUnread = true
                }
            };

            Func<Task> result = () => repo.CreateArticlesAsync(articlesToCreate);

            // Assert
            await result.Should().ThrowAsync<Exception>();

        }

        [Fact]
        public async Task UpdateArticleAsync_WhenArticleExists_UpdatesArticle()
        {
            // Arrange
            var db = CreateDb();

            var article = new Article
            {
                FeedId = Guid.NewGuid(),
                ArticleId = Guid.NewGuid(),
                ArticleTitle = "Article 1",
                ArticleContent = "Content 1",
                ArticleUrl = "https://example3.com/article1",
                ArticleDate = DateTime.UtcNow.AddDays(-2),
                IsUnread = true
            };

            db.Articles.Add(article);

            await db.SaveChangesAsync();

            db.Entry(article).State = EntityState.Detached;

            var repo = new FeedRepository(Mock.Of<ILogger<FeedRepository>>(), db);

            // Act
            var articleToUpdate = await db.Articles
                .AsNoTracking()
                .FirstOrDefaultAsync();

            articleToUpdate!.IsUnread = false;

            await repo.UpdateArticleAsync(articleToUpdate);

            // Assert
            var updatedArticle = await db.Articles
                .AsNoTracking()
                .FirstOrDefaultAsync();
            updatedArticle!.IsUnread.Should().Be(false);

        }

        [Fact]
        public async Task UpdateArticleAsync_WhenArticleDoesNotExist_ThrowsException()
        {
            // Arrange
            var db = CreateDb();
            var repo = new FeedRepository(Mock.Of<ILogger<FeedRepository>>(), db);

            // Act
            var article = new Article
            {
                FeedId = Guid.NewGuid(),
                ArticleId = Guid.NewGuid(),
                ArticleTitle = "Article 1",
                ArticleContent = "Content 1",
                ArticleUrl = "https://example3.com/article1",
                ArticleDate = DateTime.UtcNow.AddDays(-2),
                IsUnread = false
            };
            Func<Task> result = () => repo.UpdateArticleAsync(article);

            // Assert
            await result.Should().ThrowAsync<Exception>();

        }

        [Fact]
        public async Task DeleteUserFeedsAsync_WhenUserHasFeeds_DeletesFeeds()
        {
            // Arrange
            var db = CreateSqLiteDb();

            var userId = Guid.NewGuid();
            var feedId1 = Guid.NewGuid();
            var feedId2 = Guid.NewGuid();
            var feedId3 = Guid.NewGuid();

            var feed1 = new Feed
            {
                FeedId = feedId1,
                UserId = userId,
                FeedTitle = "Test 1",
                FeedUrl = "https://example1.com/rss",
                BlogUrl = "https://example1.com",
                Articles = new List<Article>
                {
                    new Article
                    {
                        FeedId = feedId1,
                        ArticleId = Guid.NewGuid(),
                        ArticleTitle = "Article 1",
                        ArticleContent = "Content 1",
                        ArticleUrl = "https://example3.com/article1",
                        ArticleDate = DateTime.UtcNow.AddDays(-2),
                        IsUnread = true
                    }
                }
            };

            db.Feeds.Add(feed1);

            var feed2 = new Feed
            {
                FeedId = feedId2,
                UserId = Guid.NewGuid(),
                FeedTitle = "Test 2",
                FeedUrl = "https://example2.com/rss",
                BlogUrl = "https://example2.com",
                Articles = new List<Article>
                {
                    new Article
                    {
                        FeedId = feedId2,
                        ArticleId = Guid.NewGuid(),
                        ArticleTitle = "Article 1",
                        ArticleContent = "Content 1",
                        ArticleUrl = "https://example2.com/article1",
                        ArticleDate = DateTime.UtcNow.AddDays(-2),
                        IsUnread = true
                    }
                }
            };

            db.Feeds.Add(feed2);

            var feed3 = new Feed
            {
                FeedId = feedId3,
                UserId = userId,
                FeedTitle = "Test 3",
                FeedUrl = "https://example3.com/rss",
                BlogUrl = "https://example3.com",
                Articles = new List<Article>
                {
                    new Article
                    {
                        FeedId = feedId3,
                        ArticleId = Guid.NewGuid(),
                        ArticleTitle = "Article 1",
                        ArticleContent = "Content 1",
                        ArticleUrl = "https://example3.com/article1",
                        ArticleDate = DateTime.UtcNow.AddDays(-2),
                        IsUnread = true
                    }
                }
            };

            db.Feeds.Add(feed3);

            await db.SaveChangesAsync();

            var repo = new FeedRepository(Mock.Of<ILogger<FeedRepository>>(), db);

            // Act
            await repo.DeleteUserFeedsAsync(userId);

            // Assert
            var feeds = await db.Feeds
                .AsNoTracking()
                .ToListAsync();
            feeds.Should().HaveCount(1);
            feeds.Should().NotContain(f => f.UserId == userId);

            var articles = await db.Articles
                .Include(a => a.Feed)
                .AsNoTracking()
                .ToListAsync();
            articles.Should().HaveCount(1);
            articles.Should().NotContain(a => a.Feed.UserId == userId);

        }

        [Fact]
        public async Task DeleteUserFeedsAsync_WhenUserHasNoFeeds_DoesNothing()
        {
            // Arrange
            var db = CreateSqLiteDb();
            
            var feedId = Guid.NewGuid();

            var feed = new Feed
            {
                FeedId = feedId,
                UserId = Guid.NewGuid(),
                FeedTitle = "Test 1",
                FeedUrl = "https://example1.com/rss",
                BlogUrl = "https://example1.com",
                Articles = new List<Article>
                {
                    new Article
                    {
                        FeedId = feedId,
                        ArticleId = Guid.NewGuid(),
                        ArticleTitle = "Article 1",
                        ArticleContent = "Content 1",
                        ArticleUrl = "https://example3.com/article1",
                        ArticleDate = DateTime.UtcNow.AddDays(-2),
                        IsUnread = true
                    }
                }
            };

            db.Feeds.Add(feed);

            await db.SaveChangesAsync();

            var repo = new FeedRepository(Mock.Of<ILogger<FeedRepository>>(), db);

            // Act
            Func<Task> result = () => repo.DeleteUserFeedsAsync(Guid.NewGuid());

            // Assert
            var feeds = await db.Feeds
                .AsNoTracking()
                .ToListAsync();
            feeds.Count.Should().Be(1);

        }

        [Fact]
        public async Task CreateFeedUpdateAsync_WhenUpdateIsValid_CreatesFeedUpdate()
        {
            // Arrange
            var db = CreateDb();

            var userId = Guid.NewGuid();

            var feedUpdate = new FeedUpdate
            {
                UserId = userId,
                RequestedAt = DateTime.UtcNow
            };

            var repo = new FeedRepository(Mock.Of<ILogger<FeedRepository>>(), db);

            // Act
            await repo.CreateFeedUpdateAsync(feedUpdate);

            // Assert
            var createdFeedUpdate = await db.FeedUpdates
                .AsNoTracking()
                .FirstOrDefaultAsync();
            createdFeedUpdate.Should().NotBeNull();
            createdFeedUpdate.UserId.Should().Be(userId);
        }

        [Fact]
        public async Task GetPendingFeedUpdatesAsync_WhenFeedUpdatesExist_ReturnsFeedUpdates()
        {
            // Arrange
            var db = CreateDb();

            db.FeedUpdates.Add(new FeedUpdate
            {
                UserId = Guid.NewGuid(),
                RequestedAt = DateTime.UtcNow
            });

            db.FeedUpdates.Add(new FeedUpdate
            {
                UserId = Guid.NewGuid(),
                RequestedAt = DateTime.UtcNow
            });

            db.FeedUpdates.Add(new FeedUpdate
            {
                UserId = Guid.NewGuid(),
                RequestedAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync();

            var repo = new FeedRepository(Mock.Of<ILogger<FeedRepository>>(), db);

            // Act
            var result = await repo.GetPendingFeedUpdatesAsync(2);

            // Assert
            result.Should().HaveCount(2);
            result.Should().BeInAscendingOrder(a => a.RequestedAt);

        }

        [Fact]
        public async Task DeleteFeedUpdatesAsync_WhenFeedUpdatesExist_DeletesFeedUpdates()
        {
            // Arrange
            var db = CreateSqLiteDb();

            var userId1 = Guid.NewGuid();
            db.FeedUpdates.Add(new FeedUpdate
            {
                UserId = userId1,
                RequestedAt = DateTime.UtcNow
            });
            db.FeedUpdates.Add(new FeedUpdate
            {
                UserId = userId1,
                RequestedAt = DateTime.UtcNow
            });
            var userId2 = Guid.NewGuid();
            db.FeedUpdates.Add(new FeedUpdate
            {
                UserId = userId2,
                RequestedAt = DateTime.UtcNow
            });
            var userId3 = Guid.NewGuid();
            db.FeedUpdates.Add(new FeedUpdate
            {
                UserId = userId3,
                RequestedAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync();

            List<Guid> userIds = new List<Guid> { userId1, userId2 };

            var repo = new FeedRepository(Mock.Of<ILogger<FeedRepository>>(), db);

            // Act
            await repo.DeleteFeedUpdatesAsync(userIds);

            // Assert
            var feedUpdates = await db.FeedUpdates
                .AsNoTracking()
                .ToListAsync();
            feedUpdates.Count.Should().Be(1);
            feedUpdates.Should().NotContain(u => userIds.Contains(u.UserId));

        }

    }
}
