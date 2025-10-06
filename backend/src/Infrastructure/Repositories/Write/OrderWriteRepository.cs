using ProductCatalogAPI.Domain.Entities;
using ProductCatalogAPI.Domain.Interfaces.Write;
using ProductCatalogAPI.Infrastructure.Data;

namespace ProductCatalogAPI.Infrastructure.Repositories.Write;

public class OrderWriteRepository : IOrderWriteRepository
{
    private readonly ApplicationDbContext _db;
    public OrderWriteRepository(ApplicationDbContext db) => _db = db;

    public Task<Order> AddAsync(Order order, CancellationToken ct = default)
    {
        _db.Set<Order>().Add(order);
        return Task.FromResult(order);
    }
}