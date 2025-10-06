namespace ProductCatalogAPI.Application.UseCases.Products;

using ProductCatalogAPI.Application.DTOs;
using ProductCatalogAPI.Application.Ports;
using ProductCatalogAPI.Domain.Entities;
using ProductCatalogAPI.Domain.Interfaces.Write;

public class CreateProductUseCase
{
    private readonly IUnitOfWork _uow;
    private readonly IProductWriteRepository _write;

    public CreateProductUseCase(IUnitOfWork uow, IProductWriteRepository write)
    { _uow = uow; _write = write; }

    public async Task<ProductDto> ExecuteAsync(CreateProductDto dto, CancellationToken ct = default)
    {
        var entity = new Product();
        entity.Name = dto.Name;
        entity.Description = dto.Description ?? string.Empty;
        entity.Price = dto.Price;
        entity.Stock = dto.Stock;
        entity.CreatedAt = DateTime.UtcNow;
        entity.Version = 0;

        await _uow.BeginTransactionAsync(ct);
        await _write.AddAsync(entity, ct);
        await _uow.CommitAsync(ct);

        return new ProductDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Price = entity.Price,
            Stock = entity.Stock
        };
    }
}
