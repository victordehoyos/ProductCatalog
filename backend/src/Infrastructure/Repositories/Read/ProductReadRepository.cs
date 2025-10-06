using Microsoft.EntityFrameworkCore;
using ProductCatalogAPI.Domain.Entities;
using ProductCatalogAPI.Domain.Interfaces.Read;
using ProductCatalogAPI.Infrastructure.Data;

namespace ProductCatalogAPI.Infrastructure.Repositories.Read;

public class ProductReadRepository : IProductReadRepository
{
    private readonly ApplicationDbContext _db;
    public ProductReadRepository(ApplicationDbContext db) => _db = db;

    public Task<Product?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Set<Product>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<List<Product>> GetAllAsync(CancellationToken ct = default)
        => _db.Set<Product>().AsNoTracking().OrderBy(p => p.Name).ToListAsync(ct);
    
    public Task<bool> ExistsAsync(int id, CancellationToken ct = default) => 
        _db.Set<Product>().AsNoTracking().AnyAsync(x => x.Id == id);

}