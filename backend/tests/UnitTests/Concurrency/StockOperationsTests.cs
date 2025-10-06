using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ProductCatalogAPI.Application.DTOs;
using ProductCatalogAPI.Application.Exceptions;
using ProductCatalogAPI.Application.Ports;
using ProductCatalogAPI.Application.UseCases.Products;
using ProductCatalogAPI.Application.UseCases.Orders;
using ProductCatalogAPI.Domain.Entities;
using ProductCatalogAPI.Domain.Interfaces.Read;
using ProductCatalogAPI.Domain.Interfaces.Write;
using ProductCatalogAPI.Infrastructure.Data;
using ProductCatalogAPI.Infrastructure.Persistence;
using ProductCatalogAPI.Infrastructure.Repositories.Read;
using ProductCatalogAPI.Infrastructure.Repositories.Write;
using ProductCatalogAPI.UnitTests.Shared.Builders;


namespace ProductCatalogAPI.UnitTests.Concurrency;

public class StockOperationsTests //: IAsyncLifetime
{/*
    private ApplicationDbContext _dbContext;
    private IUnitOfWork _uow;
    private ProductWriteRepository _productWriteRepo;
    private ProductReadRepository _productReadRepo;
    private Mock<IOrderReadRepository> _mockOrderReadRepo;
    private Mock<IOrderWriteRepository> _mockOrderWriteRepo;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"StockOperationsTest_{Guid.NewGuid()}")
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _uow = new UnitOfWork(_dbContext);
        _productWriteRepo = new ProductWriteRepository(_dbContext);
        _productReadRepo = new ProductReadRepository(_dbContext);
        _mockOrderReadRepo = new Mock<IOrderReadRepository>();
        _mockOrderWriteRepo = new Mock<IOrderWriteRepository>();

        // Seed initial data
        var product = ProductBuilder.BuildDefault();

        await _dbContext.Products.AddAsync(product);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }

    [Fact]
    public async Task DecreaseStock_Simultaneous()
    {
        var productId = 1;
        var initialStock = 10;

        // Simular múltiples hilos disminuyendo stock simultáneamente
        var tasks = new List<Task<bool>>();
        var successCount = 0;
        var concurrencyExceptions = 0;

        // Act - 5 threads intentando disminuir stock
        for (int i = 0; i < 5; i++)
        {
            var task = Task.Run(async () =>
            {
                try
                {
                    await using var context = new ApplicationDbContext(
                        new DbContextOptionsBuilder<ApplicationDbContext>()
                            .UseInMemoryDatabase(_dbContext.Database.GetDbConnection().ConnectionString)
                            .Options);

                    var uow = new UnitOfWork(context);
                    var repo = new ProductWriteRepository(context);

                    await uow.BeginTransactionAsync();
                    var result = await repo.TryDecreaseStockAdminAsync(productId, 3, CancellationToken.None);
                    
                    if (result)
                    {
                        await uow.CommitAsync();
                        Interlocked.Increment(ref successCount);
                    }
                    else
                    {
                        await uow.RollbackAsync();
                    }
                    return result;
                }
                catch (DbUpdateConcurrencyException)
                {
                    Interlocked.Increment(ref concurrencyExceptions);
                    return false;
                }
            });
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);

        // Assert
        var finalProduct = await _dbContext.Products.FindAsync(productId);
        finalProduct.Should().NotBeNull();
        
        // Verificar que el stock final sea consistente
        var expectedStock = initialStock - (successCount * 3);
        finalProduct.Stock.Should().Be(expectedStock);
        
        // Al menos una operación debería fallar por concurrencia
        concurrencyExceptions.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateOrder_WithIdempotency()
    {
        // Arrange
        var idempotencyKey = "test-key-123";
        var existingOrder = OrderBuilder.Create()
            .WithId(1)
            .WithProductId(1)
            .WithQuantity(2)
            .WithTotal(200.0m)
            .WithIdempotencyKey(idempotencyKey)
            .Build();

        _mockOrderReadRepo.Setup(r => r.GetByIdempotencyKeyAsync(idempotencyKey, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(existingOrder);

        var createOrderUseCase = new CreateOrderUseCase(
            _uow, _productReadRepo, _productWriteRepo, _mockOrderReadRepo.Object, _mockOrderWriteRepo.Object);

        var createOrderDto = new CreateOrderDto { ProductId = 1, Quantity = 2 };

        // Act - Llamar dos veces con la misma idempotency key
        var result1 = await createOrderUseCase.ExecuteAsync(createOrderDto, idempotencyKey);
        var result2 = await createOrderUseCase.ExecuteAsync(createOrderDto, idempotencyKey);

        // Assert
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result1.Id.Should().Be(result2.Id); // Misma orden
        result1.Id.Should().Be(existingOrder.Id); // Orden existente

        // Verificar que no se creó nueva orden
        _mockOrderWriteRepo.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProduct_WhileStockOperationInProgress()
    {
        // Arrange
        var productId = 1;
        
        var updateUseCase = new UpdateProductUseCase(
            _uow, _productReadRepo, _productWriteRepo, Mock.Of<ILogger<UpdateProductUseCase>>());

        var increaseStockUseCase = new IncreaseStockUseCase(_uow, _productWriteRepo);

        var updateDto = new UpdateProductDto 
        { 
            Name = "Updated Name", 
            Price = 150.0m 
        };

        // Act - Ejecutar operaciones concurrentes
        var updateTask = updateUseCase.ExecuteAsync(productId, updateDto);
        var stockTask = increaseStockUseCase.ExecuteAsync(productId, 5);

        // Assert - Una de las operaciones debería fallar
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => Task.WhenAll(updateTask, stockTask));
    }

    [Fact]
    public async Task CreateOrder_WhileProductIsBeingDeleted_ThrowsNotFoundException()
    {
        // Arrange
        var productId = 1;
        var idempotencyKey = "concurrent-delete-test";

        var createOrderUseCase = new CreateOrderUseCase(
            _uow, _productReadRepo, _productWriteRepo, _mockOrderReadRepo.Object, _mockOrderWriteRepo.Object);

        var deleteProductUseCase = new DeleteProductUseCase(_uow, _productReadRepo, _productWriteRepo);

        // Configurar mock para cuando el producto no existe (después de ser eliminado)
        _mockOrderReadRepo.Setup(r => r.GetByIdempotencyKeyAsync(idempotencyKey, It.IsAny<CancellationToken>()))
                        .ReturnsAsync((Order)null);

        // Act
        var orderTask = createOrderUseCase.ExecuteAsync(
            new CreateOrderDto { ProductId = productId, Quantity = 2 }, 
            idempotencyKey);

        var deleteTask = deleteProductUseCase.ExecuteAsync(productId);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(() => orderTask);
        await deleteTask; // Esperar a que complete la eliminación
    }

    [Fact]
    public async Task MultipleOrders_CompeteForLimitedStock_OnlySomeSucceed()
    {
        // Arrange
        var productId = 1;
        var initialStock = 5;
        var orderQuantity = 2;
        var concurrentOrders = 4;

        // Actualizar producto con stock limitado
        var product = await _dbContext.Products.FindAsync(productId);
        product.Stock = initialStock;
        await _dbContext.SaveChangesAsync();

        var successCount = 0;
        var failureCount = 0;
        var tasks = new List<Task>();

        // Act - Crear múltiples órdenes concurrentes
        for (int i = 0; i < concurrentOrders; i++)
        {
            var task = Task.Run(async () =>
            {
                try
                {
                    await using var context = new ApplicationDbContext(
                        new DbContextOptionsBuilder<ApplicationDbContext>()
                            .UseInMemoryDatabase(_dbContext.Database.GetDbConnection().ConnectionString)
                            .Options);

                    var uow = new UnitOfWork(context);
                    var productWriteRepo = new ProductWriteRepository(context);
                    var orderWriteRepo = new OrderWriteRepository(context);
                    var productReadRepo = new ProductReadRepository(context);
                    var orderReadRepo = new OrderReadRepository(context);

                    var useCase = new CreateOrderUseCase(
                        uow, productReadRepo, productWriteRepo, orderReadRepo, orderWriteRepo);

                    var idempotencyKey = Guid.NewGuid().ToString();
                    var orderDto = new CreateOrderDto 
                    { 
                        ProductId = productId, 
                        Quantity = orderQuantity 
                    };

                    await useCase.ExecuteAsync(orderDto, idempotencyKey);
                    Interlocked.Increment(ref successCount);
                }
                catch (InsufficientStockException)
                {
                    Interlocked.Increment(ref failureCount);
                }
                catch (Exception)
                {
                    Interlocked.Increment(ref failureCount);
                }
            });
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);

        // Assert
        var finalProduct = await _dbContext.Products.FindAsync(productId);
        finalProduct.Should().NotBeNull();

        // Calcular cuántas órdenes deberían haber tenido éxito
        var maxPossibleOrders = initialStock / orderQuantity; // 5 / 2 = 2 órdenes
        successCount.Should().Be(maxPossibleOrders);
        failureCount.Should().Be(concurrentOrders - maxPossibleOrders);

        // Verificar stock final
        var expectedFinalStock = initialStock - (successCount * orderQuantity);
        finalProduct.Stock.Should().Be(expectedFinalStock);
    }

    [Fact]
    public async Task DecreaseStockAdmin_WithConcurrentUpdates_PreservesDataConsistency()
    {
        // Arrange
        var productId = 1;
        var initialVersion = 1;

        // Simular múltiples operaciones admin concurrentes
        var tasks = new List<Task>();
        var completedOperations = 0;
        var versionConflicts = 0;

        // Act
        for (int i = 0; i < 3; i++)
        {
            var task = Task.Run(async () =>
            {
                try
                {
                    await using var context = new ApplicationDbContext(
                        new DbContextOptionsBuilder<ApplicationDbContext>()
                            .UseInMemoryDatabase(_dbContext.Database.GetDbConnection().ConnectionString)
                            .Options);

                    var uow = new UnitOfWork(context);
                    var repo = new ProductWriteRepository(context);
                    var useCase = new DecreaseStockAdminUseCase(uow, repo);

                    await useCase.ExecuteAsync(productId, 1, CancellationToken.None);
                    Interlocked.Increment(ref completedOperations);
                }
                catch (DbUpdateConcurrencyException)
                {
                    Interlocked.Increment(ref versionConflicts);
                }
            });
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);

        // Assert
        var finalProduct = await _dbContext.Products.FindAsync(productId);
        finalProduct.Should().NotBeNull();
        
        // La versión debería haber incrementado
        finalProduct.Version.Should().BeGreaterThan(initialVersion);
        
        // El stock debería ser consistente
        finalProduct.Stock.Should().Be(10 - completedOperations);
    }

    [Fact]
    public async Task MixedOperations_UpdateAndStockChanges_HandleConcurrencyCorrectly()
    {
        // Arrange
        var productId = 1;
        
        var updateUseCase = new UpdateProductUseCase(
            _uow, _productReadRepo, _productWriteRepo, Mock.Of<ILogger<UpdateProductUseCase>>());

        var increaseUseCase = new IncreaseStockUseCase(_uow, _productWriteRepo);
        var decreaseUseCase = new DecreaseStockAdminUseCase(_uow, _productWriteRepo);

        var updateDto = new UpdateProductDto 
        { 
            Name = "Concurrent Update", 
            Price = 200.0m 
        };

        // Act - Ejecutar operaciones mixtas concurrentes
        var tasks = new List<Task>
        {
            updateUseCase.ExecuteAsync(productId, updateDto),
            increaseUseCase.ExecuteAsync(productId, 3),
            decreaseUseCase.ExecuteAsync(productId, 2)
        };

        // Assert - Al menos una debería fallar por concurrencia
        var aggregateException = await Assert.ThrowsAsync<AggregateException>(() => Task.WhenAll(tasks));
        aggregateException.InnerExceptions.Should().Contain(ex => ex is DbUpdateConcurrencyException);
    }*/
}