using FeedApp3.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FeedApp3.Api.Data.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Feed> Feeds { get; set; }
    public DbSet<Article> Articles { get; set; }
    public DbSet<FeedUpdate> FeedUpdates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Feed configuration
        modelBuilder.Entity<Feed>(entity =>
        {
            entity.HasKey(f => f.FeedId);

            entity.Property(f => f.FeedUrl).IsRequired();

            entity.Property(f => f.FeedTitle).IsRequired();

            entity.Property(f => f.BlogUrl).IsRequired();

            entity.Property(f => f.LastChecked).IsRequired();

            entity.HasIndex(f => new { f.UserId, f.FeedUrl }).IsUnique();

            entity.HasMany(f => f.Articles)
                  .WithOne(a => a.Feed)
                  .HasForeignKey(a => a.FeedId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Ignore(f => f.UnreadArticleCount);
        });

        // Article configuration
        modelBuilder.Entity<Article>(entity =>
        {
            entity.HasKey(a => a.ArticleId);

            entity.Property(a => a.ArticleUrl).IsRequired();

            entity.Property(a => a.ArticleTitle).IsRequired();

            entity.Property(a => a.ArticleContent).IsRequired();

            entity.Property(a => a.ArticleDate).IsRequired();

            entity.Property(a => a.IsUnread).IsRequired();

            entity.HasIndex(a => new { a.FeedId, a.ArticleUrl }).IsUnique();
        });

    }
}