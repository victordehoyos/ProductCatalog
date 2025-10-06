using Microsoft.EntityFrameworkCore;
using ProductCatalogAPI.Domain.Entities;
using ProductCatalogAPI.Domain.Interfaces.Write;
using ProductCatalogAPI.Infrastructure.Data;

namespace ProductCatalogAPI.Infrastructure.Repositories.Write;

public class ProductWriteRepository : IProductWriteRepository
{
    private readonly ApplicationDbContext _db;
    public ProductWriteRepository(ApplicationDbContext db) => _db = db;

    public Task<Product> AddAsync(Product product, CancellationToken ct = default)
    {
        _db.Set<Product>().Add(product);
        return Task.FromResult(product);
    }
    
    public Task<Product?> GetTrackedByIdAsync(int id, CancellationToken ct = default)
        => _db.Set<Product>().FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Product> UpdateAsync(Product product, CancellationToken ct = default)
    {
        product.Version++;
        return Task.FromResult(product);
    }

    public async Task<bool> TryDecreaseStockAdminAsync(int productId, int qty, CancellationToken ct = default)
    {
        var rows = await _db.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE Products
            SET Stock = Stock - {qty}, Version = Version + 1
            WHERE Id = {productId} AND Stock >= {qty};", ct);
        return rows > 0;
    }

    public Task IncreaseStockAsync(int productId, int qty, CancellationToken ct = default)
        => _db.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE Products
            SET Stock = Stock + {qty}, Version = Version + 1
            WHERE Id = {productId};", ct);

    public Task DeleteAsync(Product product, CancellationToken ct = default)
    {
        _db.Set<Product>().Remove(product);
        return Task.CompletedTask;
    }
    
}

