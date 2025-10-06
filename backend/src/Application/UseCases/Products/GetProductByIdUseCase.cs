using ProductCatalogAPI.Application.DTOs;
using ProductCatalogAPI.Application.Exceptions;
using ProductCatalogAPI.Domain.Interfaces.Read;

namespace ProductCatalogAPI.Application.UseCases.Products;

public class GetProductByIdUseCase
{
    private readonly IProductReadRepository _read;

    public GetProductByIdUseCase(IProductReadRepository read) => _read = read;

    public async Task<ProductDto?> ExecuteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _read.GetByIdAsync(id, ct);
        
        if (entity is not null)
        {
            return new ProductDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                Price = entity.Price,
                Stock = entity.Stock
            };
        }

        return null;
    }
}