using ProductCatalogAPI.Application.Ports;
using ProductCatalogAPI.Domain.Interfaces.Write;

namespace ProductCatalogAPI.Application.UseCases.Products;

public class IncreaseStockUseCase
{
    private readonly IUnitOfWork _uow;
    private readonly IProductWriteRepository _write;

    public IncreaseStockUseCase(IUnitOfWork uow, IProductWriteRepository write)
    {
        _uow = uow; 
        _write = write;
    }

    public async Task ExecuteAsync(int productId, int qty, CancellationToken ct = default)
    {
        await _uow.BeginTransactionAsync(ct);
        await _write.IncreaseStockAsync(productId, qty, ct);
        await _uow.CommitAsync(ct);
    }
}