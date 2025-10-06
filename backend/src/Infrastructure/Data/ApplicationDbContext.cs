using Microsoft.EntityFrameworkCore;
using ProductCatalogAPI.Domain.Entities;

namespace ProductCatalogAPI.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(p =>
        {
            p.Property(x => x.Name).HasMaxLength(100).IsRequired();
            p.Property(x => x.Description).HasMaxLength(500);
            p.Property(x => x.Version).IsConcurrencyToken();     // optimist token
        });

        modelBuilder.Entity<Order>(o =>
        {
            o.Property(x => x.IdempotencyKey).HasMaxLength(64);
            o.HasIndex(x => x.IdempotencyKey).IsUnique();        // idempotency
        });
    }
}