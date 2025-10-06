using ProductCatalogAPI.Infrastructure.Repositories.Read;

namespace ProductCatalogAPI.IntegrationTests.Repositories;

using Microsoft.EntityFrameworkCore;
using ProductCatalogAPI.Domain.Entities;
using ProductCatalogAPI.Infrastructure.Data;

public class ProductReadRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ProductReadRepository _repository;

    public ProductReadRepositoryTests()
    {
        // Configurar BD en memoria
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"ProductTestDb_{Guid.NewGuid()}")
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new ProductReadRepository(_context);
        
        SeedTestData();
    }

    private void SeedTestData()
    {
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Zapatos", Price = 50.0m, Stock = 10, Description = "Zapatos deportivos" },
            new() { Id = 2, Name = "Camiseta", Price = 25.0m, Stock = 20, Description = "Camiseta de algodón" },
            new() { Id = 3, Name = "Pantalón", Price = 35.0m, Stock = 15, Description = "Pantalón jeans" },
            new() { Id = 4, Name = "Laptop", Price = 1000.0m, Stock = 5, Description = "Laptop gaming" }
        };

        _context.Products.AddRange(products);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context?.Database.EnsureDeleted();
        _context?.Dispose();
    }
    
    [Fact]
    public async Task GetByIdAsync_ExistingProduct_ReturnsProduct()
    {
        // Arrange
        var productId = 1;
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _repository.GetByIdAsync(productId, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(productId, result.Id);
        Assert.Equal("Zapatos", result.Name);
        Assert.Equal(50.0m, result.Price);
        Assert.Equal(10, result.Stock);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingProduct_ReturnsNull()
    {
        // Arrange
        var nonExistingProductId = 999;
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _repository.GetByIdAsync(nonExistingProductId, cancellationToken);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_WithProducts_ReturnsAllProductsOrderedByName()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _repository.GetAllAsync(cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.Count);
        
        // Verificar que está ordenado por nombre ascendente
        var expectedOrder = new[] { "Camiseta", "Laptop", "Pantalón", "Zapatos" };
        var actualOrder = result.Select(p => p.Name).ToArray();
        
        Assert.Equal(expectedOrder, actualOrder);
    }

    [Fact]
    public async Task GetAllAsync_EmptyDatabase_ReturnsEmptyList()
    {
        // Arrange - Crear nuevo contexto vacío
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"EmptyProductDb_{Guid.NewGuid()}")
            .Options;
        
        var emptyContext = new ApplicationDbContext(options);
        var emptyRepository = new ProductReadRepository(emptyContext);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await emptyRepository.GetAllAsync(cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);

        emptyContext.Dispose();
    }
    
    [Fact]
    public async Task ExistsAsync_ExistingProduct_ReturnsTrue()
    {
        // Arrange
        var existingProductId = 1;
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _repository.ExistsAsync(existingProductId, cancellationToken);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_NonExistingProduct_ReturnsFalse()
    {
        // Arrange
        var nonExistingProductId = 999;
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _repository.ExistsAsync(nonExistingProductId, cancellationToken);

        // Assert
        Assert.False(result);
    }
    
}