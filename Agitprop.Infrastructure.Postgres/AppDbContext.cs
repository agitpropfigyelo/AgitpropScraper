using Agitprop.Infrastructure.Postgres.Models;

using Microsoft.EntityFrameworkCore;

namespace Agitprop.Infrastructure.Postgres;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) {}

    public DbSet<PostgresArticle> Articles => Set<PostgresArticle>();
    public DbSet<PostgresEntity> Entities => Set<PostgresEntity>();
    public DbSet<PostgresMention> Mentions => Set<PostgresMention>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<PostgresMention>()
          .HasOne(m => m.Article).WithMany(a => a.Mentions).HasForeignKey(m => m.ArticleId);
        mb.Entity<PostgresMention>()
          .HasOne(m => m.Entity).WithMany(e => e.Mentions).HasForeignKey(m => m.EntityId);

        mb.Entity<PostgresArticle>().HasIndex(a => a.PublishedTime);
        // further constraints/indexes as needed
    }
}
