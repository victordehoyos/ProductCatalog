using Microsoft.EntityFrameworkCore;
using ProductCatalogAPI.Domain.Entities;
using ProductCatalogAPI.Domain.Interfaces.Read;
using ProductCatalogAPI.Infrastructure.Data;

namespace ProductCatalogAPI.Infrastructure.Repositories.Read;

public class OrderReadRepository : IOrderReadRepository
{
    private readonly ApplicationDbContext _db;
    public OrderReadRepository(ApplicationDbContext db) => _db = db;

    public Task<Order?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Set<Order>().AsNoTracking().FirstOrDefaultAsync(o => o.Id == id, ct);

    public Task<Order?> GetByIdempotencyKeyAsync(string key, CancellationToken ct = default)
        => _db.Set<Order>().AsNoTracking().FirstOrDefaultAsync(o => o.IdempotencyKey == key, ct);

    public Task<List<Order>> GetByProductIdAsync(int productId, CancellationToken ct = default)
        => _db.Set<Order>().AsNoTracking().Where(o => o.ProductId == productId).ToListAsync(ct);
    
    public Task<List<Order>> GetAllAsync(CancellationToken ct = default)
        => _db.Set<Order>().AsNoTracking().OrderByDescending((o => o.Date)).ToListAsync(ct);
}