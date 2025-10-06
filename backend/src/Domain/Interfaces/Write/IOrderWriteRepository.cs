using ProductCatalogAPI.Domain.Entities;

namespace ProductCatalogAPI.Domain.Interfaces.Write;

public interface IOrderWriteRepository
{
    Task<Order> AddAsync(Order order, CancellationToken ct = default);
}