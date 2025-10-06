using ProductCatalogAPI.Domain.Entities;

namespace ProductCatalogAPI.Domain.Interfaces.Read;

public interface IOrderReadRepository
{
    Task<Order?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<Order>> GetAllAsync(CancellationToken ct = default);
    Task<List<Order>> GetByProductIdAsync(int productId, CancellationToken ct = default);
    Task<Order?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct = default);
}