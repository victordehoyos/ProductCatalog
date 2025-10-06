using ProductCatalogAPI.Application.DTOs;
using ProductCatalogAPI.Domain.Interfaces.Read;

namespace ProductCatalogAPI.Application.UseCases.Products;

public class GetAllProductsUseCase
{
    private readonly IProductReadRepository _read;

    public GetAllProductsUseCase(IProductReadRepository read) => _read = read;

    public async Task<List<ProductDto>> ExecuteAsync(CancellationToken ct = default)
    {
        var items = await _read.GetAllAsync(ct);
        return items.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Stock = p.Stock
        }).ToList();
    }
}