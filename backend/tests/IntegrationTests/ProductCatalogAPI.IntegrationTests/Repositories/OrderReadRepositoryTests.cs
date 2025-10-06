using ProductCatalogAPI.Domain.Entities;
using ProductCatalogAPI.Infrastructure.Repositories.Read;

namespace ProductCatalogAPI.IntegrationTests.Repositories;

using Microsoft.EntityFrameworkCore;
using ProductCatalogAPI.Infrastructure.Data;

public class OrderReadRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly OrderReadRepository _repository;

    public OrderReadRepositoryTests()
    {
        // Configurar BD en memoria
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}") // Nombre único por test
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new OrderReadRepository(_context);
        
        // Opcional: inicializar datos comunes
        SeedTestData();
    }

    private void SeedTestData()
    {
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product 1", Price = 10.0m, Stock = 100 },
            new() { Id = 2, Name = "Product 2", Price = 20.0m, Stock = 50 }
        };

        var orders = new List<Order>
        {
            new() { Id = 1, ProductId = 1, Quantity = 2, Total = 20.0m, Date = DateTime.UtcNow.AddDays(-1), IdempotencyKey = "key1" },
            new() { Id = 2, ProductId = 1, Quantity = 1, Total = 10.0m, Date = DateTime.UtcNow.AddDays(-2), IdempotencyKey = "key2" },
            new() { Id = 3, ProductId = 2, Quantity = 3, Total = 60.0m, Date = DateTime.UtcNow, IdempotencyKey = "key3" },
            new() { Id = 4, ProductId = 2, Quantity = 1, Total = 20.0m, Date = DateTime.UtcNow.AddHours(-1), IdempotencyKey = "key4" }
        };

        _context.Products.AddRange(products);
        _context.Orders.AddRange(orders);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context?.Database.EnsureDeleted();
        _context?.Dispose();
    }
    
    [Fact]
    public async Task GetByIdAsync_ExistingOrder_ReturnsOrder()
    {
        // Arrange
        var orderId = 1;
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _repository.GetByIdAsync(orderId, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderId, result.Id);
        Assert.Equal(1, result.ProductId);
        Assert.Equal(2, result.Quantity);
    }
    
    [Fact]
    public async Task GetByIdAsync_NonExistingOrder_ReturnsNull()
    {
        // Arrange
        var nonExistingOrderId = 999;
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _repository.GetByIdAsync(nonExistingOrderId, cancellationToken);

        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task GetByIdAsync_WithCancellation_ThrowsOnCancelled()
    {
        // Arrange
        var orderId = 1;
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _repository.GetByIdAsync(orderId, cts.Token));
    }
    
    [Fact]
    public async Task GetAllAsync_WithOrders_ReturnsAllOrdersOrderedByDateDesc()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _repository.GetAllAsync(cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.Count); // 4 órdenes en total
        
        // Verificar que está ordenado por fecha descendente
        for (int i = 0; i < result.Count - 1; i++)
        {
            Assert.True(result[i].Date >= result[i + 1].Date);
        }
    }
    
    [Fact]
    public async Task GetAllAsync_EmptyDatabase_ReturnsEmptyList()
    {
        // Arrange - Crear nuevo contexto vacío
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"EmptyDb_{Guid.NewGuid()}")
            .Options;
        
        var emptyContext = new ApplicationDbContext(options);
        var emptyRepository = new OrderReadRepository(emptyContext);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await emptyRepository.GetAllAsync(cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);

        emptyContext.Dispose();
    }
    
    [Fact]
    public async Task GetByProductIdAsync_ExistingProduct_ReturnsOrders()
    {
        // Arrange
        var productId = 1;
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _repository.GetByProductIdAsync(productId, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count); // 2 órdenes para productId 1
        Assert.All(result, order => Assert.Equal(productId, order.ProductId));
    }

    [Fact]
    public async Task GetByProductIdAsync_NonExistingProduct_ReturnsEmptyList()
    {
        // Arrange
        var nonExistingProductId = 999;
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _repository.GetByProductIdAsync(nonExistingProductId, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByProductIdAsync_ProductWithNoOrders_ReturnsEmptyList()
    {
        // Arrange - Agregar un producto sin órdenes
        var newProduct = new Product { Id = 3, Name = "Product 3", Price = 30.0m, Stock = 10 };
        _context.Products.Add(newProduct);
        await _context.SaveChangesAsync();

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _repository.GetByProductIdAsync(3, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    
    [Fact]
    public async Task GetByIdempotencyKeyAsync_ExistingKey_ReturnsOrder()
    {
        // Arrange
        var existingKey = "key1";
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _repository.GetByIdempotencyKeyAsync(existingKey, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existingKey, result.IdempotencyKey);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetByIdempotencyKeyAsync_NonExistingKey_ReturnsNull()
    {
        // Arrange
        var nonExistingKey = "non-existing-key";
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _repository.GetByIdempotencyKeyAsync(nonExistingKey, cancellationToken);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdempotencyKeyAsync_CaseSensitiveKey_ReturnsCorrectOrder()
    {
        // Arrange
        var upperCaseKey = "KEY1"; // diferente del "key1" en los datos
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _repository.GetByIdempotencyKeyAsync(upperCaseKey, cancellationToken);

        // Assert
        Assert.Null(result); // Debería ser case sensitive
    }
}