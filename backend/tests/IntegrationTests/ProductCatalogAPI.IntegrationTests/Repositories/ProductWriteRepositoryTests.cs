using ProductCatalogAPI.Infrastructure.Repositories.Write;

namespace ProductCatalogAPI.IntegrationTests.Repositories;

using Microsoft.EntityFrameworkCore;
using ProductCatalogAPI.Domain.Entities;
using ProductCatalogAPI.Infrastructure.Data;

public class ProductWriteRepositoryTests : IDisposable
{
    protected readonly ApplicationDbContext _context;
    protected readonly ProductWriteRepository _repository;

    public ProductWriteRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"ProductWriteTestDb_{Guid.NewGuid()}")
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new ProductWriteRepository(_context);
        
        SeedTestData();
    }

    protected virtual void SeedTestData()
    {
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product 1", Price = 10.0m, Stock = 100, Version = 1 },
            new() { Id = 2, Name = "Product 2", Price = 20.0m, Stock = 50, Version = 1 },
            new() { Id = 3, Name = "Low Stock", Price = 15.0m, Stock = 5, Version = 1 },
            new() { Id = 4, Name = "Zero Stock", Price = 25.0m, Stock = 0, Version = 1 }
        };

        _context.Products.AddRange(products);
        _context.SaveChanges();
        
        // Detach entities para simular comportamiento real
        foreach (var product in products)
        {
            _context.Entry(product).State = EntityState.Detached;
        }
    }

    public void Dispose()
    {
        _context?.Database.EnsureDeleted();
        _context?.Dispose();
    }
    
    [Fact]
    public async Task AddAsync_ValidProduct_AddsToContextAndReturnsProduct()
    {
        // Arrange
        var newProduct = new Product
        {
            Name = "New Product",
            Price = 30.0m,
            Stock = 25,
            Description = "Test Description"
        };
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _repository.AddAsync(newProduct, cancellationToken);

        // Assert
        Assert.Same(newProduct, result);
        var productInContext = _context.Set<Product>().Local.FirstOrDefault(p => p.Name == "New Product");
        Assert.NotNull(productInContext);
        Assert.Same(newProduct, productInContext);
        Assert.Equal(EntityState.Added, _context.Entry(newProduct).State);
    }
    
    [Fact]
    public async Task GetTrackedByIdAsync_ExistingProduct_ReturnsTrackedProduct()
    {
        // Arrange
        var productId = 1;
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _repository.GetTrackedByIdAsync(productId, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(productId, result.Id);
        Assert.Equal("Product 1", result.Name);
        Assert.Equal(EntityState.Unchanged, _context.Entry(result).State); // Debería estar tracked
    }
    
    [Fact]
    public async Task GetTrackedByIdAsync_NonExistingProduct_ReturnsNull()
    {
        // Arrange
        var nonExistingId = 999;
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _repository.GetTrackedByIdAsync(nonExistingId, cancellationToken);

        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task UpdateAsync_TrackedProduct_IncrementsVersion()
    {
        // Arrange
        var product = await _repository.GetTrackedByIdAsync(1, CancellationToken.None);
        var originalVersion = product.Version;

        // Act
        var result = await _repository.UpdateAsync(product);

        // Assert
        Assert.Same(product, result);
        Assert.Equal(originalVersion + 1, product.Version);
    }
    
    [Fact]
    public async Task UpdateAsync_MultipleUpdates_IncrementsVersionEachTime()
    {
        // Arrange
        var product = await _repository.GetTrackedByIdAsync(1, CancellationToken.None);
        var originalVersion = product.Version;

        // Act - Múltiples actualizaciones
        await _repository.UpdateAsync(product); // v1 -> v2
        await _repository.UpdateAsync(product); // v2 -> v3
        await _repository.UpdateAsync(product); // v3 -> v4

        // Assert
        Assert.Equal(originalVersion + 3, product.Version);
    }
    
    [Fact]
    public async Task DeleteAsync_TrackedProduct_MarksAsDeleted()
    {
        // Arrange
        var product = await _repository.GetTrackedByIdAsync(1, CancellationToken.None);
        var cancellationToken = CancellationToken.None;

        // Act
        await _repository.DeleteAsync(product, cancellationToken);

        // Assert
        Assert.Equal(EntityState.Deleted, _context.Entry(product).State);
    }

    [Fact]
    public async Task DeleteAsync_DetachedProduct_ThrowsExceptionOnSave()
    {
        // Arrange
        var detachedProduct = new Product
        {
            Id = 1,
            Name = "Detached",
            Price = 10.0m,
            Stock = 100
        };
        var cancellationToken = CancellationToken.None;

        // Act
        await _repository.DeleteAsync(detachedProduct, cancellationToken);

        // Assert - Debería fallar al guardar
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
        {
            await _context.SaveChangesAsync(cancellationToken);
        });
    }

    [Fact]
    public async Task DeleteAsync_FollowedBySaveChanges_RemovesFromDatabase()
    {
        // Arrange
        var product = await _repository.GetTrackedByIdAsync(1, CancellationToken.None);
        var cancellationToken = CancellationToken.None;

        // Act
        await _repository.DeleteAsync(product, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Assert
        var deletedProduct = await _context.Set<Product>()
            .FirstOrDefaultAsync(p => p.Id == 1, cancellationToken);
            
        Assert.Null(deletedProduct);
    }

    [Fact]
    public async Task DeleteAsync_NullProduct_ThrowsArgumentNullException()
    {
        // Arrange
        Product nullProduct = null;
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _repository.DeleteAsync(nullProduct, cancellationToken));
    }
}