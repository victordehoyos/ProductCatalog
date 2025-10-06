using ProductCatalogAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using Microsoft.EntityFrameworkCore;
using ProductCatalogAPI.Application.Ports;

namespace ProductCatalogAPI.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _db;
    private IDbContextTransaction? _tx;

    public UnitOfWork(ApplicationDbContext db) => _db = db;

    public async Task BeginTransactionAsync(CancellationToken ct = default)
        => _tx = await _db.Database.BeginTransactionAsync(ct);

    public async Task CommitAsync(CancellationToken ct = default)
    {
        try
        {
            await _db.SaveChangesAsync(ct);
            if (_tx is not null)
                await _tx.CommitAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new DBConcurrencyException("Conflicto de concurrencia detectado.");
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task RollbackAsync(CancellationToken ct = default)
        => await (_tx?.RollbackAsync(ct) ?? Task.CompletedTask);
}
