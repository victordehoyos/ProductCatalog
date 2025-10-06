using ProductCatalogAPI.Infrastructure.Repositories.Write;

namespace ProductCatalogAPI.IntegrationTests.Repositories;

using Microsoft.EntityFrameworkCore;
using ProductCatalogAPI.Domain.Entities;
using ProductCatalogAPI.Infrastructure.Data;

public class OrderWriteRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly OrderWriteRepository _repository;

    public OrderWriteRepositoryTests()
    {
        // Configurar BD en memoria
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"OrderWriteTestDb_{Guid.NewGuid()}")
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new OrderWriteRepository(_context);
        
        SeedTestData();
    }

    private void SeedTestData()
    {
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product 1", Price = 10.0m, Stock = 100 },
            new() { Id = 2, Name = "Product 2", Price = 20.0m, Stock = 50 }
        };

        var existingOrders = new List<Order>
        {
            new() { Id = 1, ProductId = 1, Quantity = 2, Total = 20.0m, Date = DateTime.UtcNow.AddDays(-1), IdempotencyKey = "existing-key-1" },
            new() { Id = 2, ProductId = 2, Quantity = 1, Total = 20.0m, Date = DateTime.UtcNow.AddDays(-2), IdempotencyKey = "existing-key-2" }
        };

        _context.Products.AddRange(products);
        _context.Orders.AddRange(existingOrders);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context?.Database.EnsureDeleted();
        _context?.Dispose();
    }
    
    [Fact]
    public async Task AddAsync_ValidOrder_AddsToContextAndReturnsOrder()
    {
        // Arrange
        var newOrder = new Order
        {
            ProductId = 1,
            Quantity = 3,
            Total = 30.0m,
            Date = DateTime.UtcNow,
            IdempotencyKey = "new-key-123"
        };
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _repository.AddAsync(newOrder, cancellationToken);

        // Assert - Verificar que retorna la misma instancia
        Assert.Same(newOrder, result);
        
        // Verificar que se agregó al contexto (pero no guardado aún)
        var orderInContext = _context.Set<Order>().Local.FirstOrDefault(o => o.IdempotencyKey == "new-key-123");
        Assert.NotNull(orderInContext);
        Assert.Same(newOrder, orderInContext);
        Assert.Equal(EntityState.Added, _context.Entry(newOrder).State);
    }
    
    [Fact]
    public async Task AddAsync_MultipleOrders_AllAddedToContext()
    {
        // Arrange
        var orders = new[]
        {
            new Order { ProductId = 1, Quantity = 1, Total = 10.0m, Date = DateTime.UtcNow, IdempotencyKey = "multi-1" },
            new Order { ProductId = 2, Quantity = 2, Total = 40.0m, Date = DateTime.UtcNow, IdempotencyKey = "multi-2" },
            new Order { ProductId = 1, Quantity = 1, Total = 10.0m, Date = DateTime.UtcNow, IdempotencyKey = "multi-3" }
        };
        var cancellationToken = CancellationToken.None;

        // Act
        foreach (var order in orders)
        {
            await _repository.AddAsync(order, cancellationToken);
        }

        // Assert
        var addedOrders = _context.Set<Order>().Local
            .Where(o => o.IdempotencyKey != null && o.IdempotencyKey.StartsWith("multi-"))
            .ToList();
            
        Assert.Equal(3, addedOrders.Count);
        Assert.All(addedOrders, order => 
            Assert.Equal(EntityState.Added, _context.Entry(order).State));
    }

    [Fact]
    public async Task AddAsync_FollowedBySaveChanges_PersistsToDatabase()
    {
        // Arrange
        var newOrder = new Order
        {
            ProductId = 1,
            Quantity = 4,
            Total = 40.0m,
            Date = DateTime.UtcNow,
            IdempotencyKey = "persist-test-key"
        };
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _repository.AddAsync(newOrder, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Assert - Verificar que se persiste en la BD
        var persistedOrder = await _context.Set<Order>()
            .FirstOrDefaultAsync(o => o.IdempotencyKey == "persist-test-key", cancellationToken);
            
        Assert.NotNull(persistedOrder);
        Assert.Equal(4, persistedOrder.Quantity);
        Assert.Equal(40.0m, persistedOrder.Total);
        Assert.Equal(1, persistedOrder.ProductId);
        Assert.True(persistedOrder.Id > 0); // Debería tener ID generado
    }

    [Fact]
    public async Task AddAsync_WithoutSaveChanges_NotPersistedToDatabase()
    {
        // Arrange
        var newOrder = new Order
        {
            ProductId = 1,
            Quantity = 1,
            Total = 10.0m,
            Date = DateTime.UtcNow,
            IdempotencyKey = "no-save-test-key"
        };
        var cancellationToken = CancellationToken.None;

        // Act - Solo agregar, no guardar
        var result = await _repository.AddAsync(newOrder, cancellationToken);

        // Assert - No debería estar en la BD (solo en el contexto local)
        var ordersInDatabase = await _context.Set<Order>()
            .Where(o => o.IdempotencyKey == "no-save-test-key")
            .ToListAsync(cancellationToken);
            
        Assert.Empty(ordersInDatabase);
        
        // Pero debería estar en el tracking local
        var orderInLocal = _context.Set<Order>().Local
            .FirstOrDefault(o => o.IdempotencyKey == "no-save-test-key");
        Assert.NotNull(orderInLocal);
    }
    
}