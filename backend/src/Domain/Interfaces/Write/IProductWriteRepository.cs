using ProductCatalogAPI.Domain.Entities;

namespace ProductCatalogAPI.Domain.Interfaces.Write;

public interface IProductWriteRepository
{
    Task<Product> AddAsync(Product product, CancellationToken ct = default); 
    Task DeleteAsync(Product product, CancellationToken ct = default);
    Task<bool> TryDecreaseStockAdminAsync(int productId, int quantity, CancellationToken ct = default);
    Task IncreaseStockAsync(int productId, int quantity, CancellationToken ct = default);
    Task<Product> UpdateAsync(Product product, CancellationToken ct = default);
    Task<Product?> GetTrackedByIdAsync(int id, CancellationToken ct = default);
}