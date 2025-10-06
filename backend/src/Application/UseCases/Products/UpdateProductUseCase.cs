using System.Data;
using Microsoft.Extensions.Logging;
using ProductCatalogAPI.Application.DTOs;
using ProductCatalogAPI.Application.Exceptions;
using ProductCatalogAPI.Application.Mappings;
using ProductCatalogAPI.Application.Ports;
using ProductCatalogAPI.Domain.Interfaces.Read;
using ProductCatalogAPI.Domain.Interfaces.Write;

namespace ProductCatalogAPI.Application.UseCases.Products;

public class UpdateProductUseCase
{
    private readonly IUnitOfWork _uow;
    private readonly IProductReadRepository _read;
    private readonly IProductWriteRepository _write;
    private readonly ILogger<UpdateProductUseCase> _logger;

    public UpdateProductUseCase(IUnitOfWork uow, IProductReadRepository read, IProductWriteRepository write,  
        ILogger<UpdateProductUseCase> logger)
    {
        _uow = uow; 
        _read = read; 
        _write = write;
        _logger = logger;
    }

    public async Task<ProductDto?> ExecuteAsync(int id, UpdateProductDto dto, CancellationToken ct = default)
    {
        var product = await _write.GetTrackedByIdAsync(id, ct) ?? throw new NotFoundException("Producto no existe o fue eliminado por otro proceso");

        product.Name = dto.Name;
        product.Description = dto.Description ?? string.Empty;
        product.Price = dto.Price;
        product.UpdatedAt = DateTime.UtcNow;

        await _uow.BeginTransactionAsync(ct);
        await _write.UpdateAsync(product, ct); 
        await _uow.CommitAsync(ct);
        
        return ProductMapper.ToDto(product); 
    }
}