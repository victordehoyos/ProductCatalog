using ProductCatalogAPI.Application.Ports;
using ProductCatalogAPI.Infrastructure.Data;

namespace ProductCatalogAPI.UnitTests.Shared;

public class TestUnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public TestUnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task BeginTransactionAsync(CancellationToken ct = default)
    {
        // No hacer nada - las transacciones no son soportadas en InMemory
        return Task.CompletedTask;
    }

    public Task CommitAsync(CancellationToken ct = default)
    {
        // Guardar cambios sin transacción
        return _context.SaveChangesAsync(ct);
    }

    public Task RollbackAsync(CancellationToken ct = default)
    {
        // No hacer nada - no hay transacción que revertir
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}