using ProductCatalogAPI.Application.Ports;
using ProductCatalogAPI.Domain.Interfaces.Read;
using ProductCatalogAPI.Domain.Interfaces.Write;

namespace ProductCatalogAPI.Application.UseCases.Products;

public class DeleteProductUseCase
{
    private readonly IUnitOfWork _uow;
    private readonly IProductReadRepository _read;
    private readonly IProductWriteRepository _write;

    public DeleteProductUseCase(
        IUnitOfWork uow,
        IProductReadRepository read,
        IProductWriteRepository write)
    {
        _uow = uow; 
        _read = read; 
        _write = write;
    }

    public async Task<bool> ExecuteAsync(int id, CancellationToken ct = default)
    {
        var p = await _read.GetByIdAsync(id, ct);
        if (p is null) return false;
        
        await _uow.BeginTransactionAsync(ct);
        await _write.DeleteAsync(p, ct);   
        await _uow.CommitAsync(ct);
        return true;
    }
}