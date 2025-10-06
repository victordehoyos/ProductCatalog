using ProductCatalogAPI.Domain.Entities;

namespace ProductCatalogAPI.Domain.Interfaces.Read;

public interface IProductReadRepository
{
    Task<Product?> GetByIdAsync(int id,  CancellationToken ct = default);
    Task<List<Product>> GetAllAsync(CancellationToken ct = default);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);

}