using ProductCatalogAPI.Application.Ports;
using ProductCatalogAPI.Domain.Interfaces.Write;

namespace ProductCatalogAPI.Application.UseCases.Products;

public class DecreaseStockAdminUseCase
{
    private readonly IUnitOfWork _uow;
    private readonly IProductWriteRepository _write; 
    
    public DecreaseStockAdminUseCase(IUnitOfWork uow, IProductWriteRepository write)
    {
        _uow = uow; 
        _write = write;
    }
    
    public async Task ExecuteAsync(int productId, int qty, CancellationToken ct = default)
    {
        await _uow.BeginTransactionAsync(ct);
        await _write.TryDecreaseStockAdminAsync(productId, qty, ct);
        await _uow.CommitAsync(ct);
    }
}