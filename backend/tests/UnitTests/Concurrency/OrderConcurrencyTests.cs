using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ProductCatalogAPI.Application.DTOs;
using ProductCatalogAPI.Application.Exceptions;
using ProductCatalogAPI.Application.Ports;
using ProductCatalogAPI.Application.UseCases.Orders;
using ProductCatalogAPI.Application.UseCases.Products;
using ProductCatalogAPI.Domain.Entities;
using ProductCatalogAPI.Infrastructure.Data;
using ProductCatalogAPI.Infrastructure.Persistence;
using ProductCatalogAPI.Infrastructure.Repositories.Read;
using ProductCatalogAPI.Infrastructure.Repositories.Write;
using ProductCatalogAPI.UnitTests.Shared;
using FluentAssertions;

namespace ProductCatalogAPI.UnitTests.Concurrency;

public class OrderConcurrencyTests //: IAsyncLifetime
{/*
    //private DbContextOptions<ApplicationDbContext> _dbContext;
    private IUnitOfWork _uow;
    private string _databaseName;
    private SqliteConnection _connection;
    private DbContextOptions<ApplicationDbContext> _dbOptions;
    
    public async Task InitializeAsync()
    {
        // Crear y abrir conexión SQLite en memoria
        _connection = new SqliteConnection("DataSource=:memory:");
        await _connection.OpenAsync();

        _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        // Crear la base de datos y datos de prueba
        await using var context = new ApplicationDbContext(_dbOptions);
        await context.Database.EnsureCreatedAsync();

        var product = new Product
        {
            Id = 1,
            Name = "Test Product",
            Description = "Test Description",
            Price = 100.0m,
            Stock = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Version = 0
        };
        
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();
    }
    
    public async Task DisposeAsync()
    {
        await _connection.CloseAsync();
        await _connection.DisposeAsync();
    }
    
    /*
    public async Task InitializeAsync()
    {
        _databaseName = $"OrderConcurrencyTest_{Guid.NewGuid()}";
        _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        // Inicializar la base de datos con datos de prueba
        await using var context = new ApplicationDbContext(_dbOptions);
        var product = new Product
        {
            Id = 1,
            Name = "Test Product",
            Description = "Test Description",
            Price = 100.0m,
            Stock = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Version = 0
        };
        
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();
    }
    /*
    public async Task InitializeAsync()
    {
        _databaseName = $"OrderConcurrencyTest_{Guid.NewGuid()}";
        _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options;
        
        await using var context = new ApplicationDbContext(_dbOptions);
        var product = new Product
        {
            Id = 1,
            Name = "Test Product",
            Description = "Test Description",
            Price = 100.0m,
            Stock = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Version = 0
        };
        
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();
        
        /*var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"OrderConcurrencyTest_{Guid.NewGuid()}")
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _uow = new UnitOfWork(_dbContext);
        
        var product = new Product
        {
            Id = 1,
            Name = "Test Product",
            Description = "Test Description",
            Price = 100.0m,
            Stock = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Version = 0
        };
        
        //var product = ProductBuilder.BuildDefault();
        await _dbContext.Products.AddAsync(product);
        await _dbContext.SaveChangesAsync();*/
    //}
    /*public Task DisposeAsync()
    {
        //await _dbContext.DisposeAsync();
        return Task.CompletedTask;
    }*/
   /* 
    [Fact]
    public async Task CreateOrder_WhileDeleted_ShouldThrowNotFoundException()
    {
        // Arrange
        var createOrderTask = Task.Run(async () =>
        {
            await using var context1 = new ApplicationDbContext(_dbOptions);
            var uow1 = new UnitOfWork(context1);
            var productReadRepo1 = new ProductReadRepository(context1);
            var productWriteRepo1 = new ProductWriteRepository(context1);
            var orderReadRepo1 = new OrderReadRepository(context1);
            var orderWriteRepo1 = new OrderWriteRepository(context1);

            var createOrderUseCase = new CreateOrderUseCase(
                uow1, productReadRepo1, productWriteRepo1, orderReadRepo1, orderWriteRepo1);

            // Pequeña pausa para asegurar que el delete comience primero
            await Task.Delay(100);
            
            return await createOrderUseCase.ExecuteAsync(
                new CreateOrderDto { ProductId = 1, Quantity = 1 }, "order_key");
        });

        var deleteProductTask = Task.Run(async () =>
        {
            await using var context2 = new ApplicationDbContext(_dbOptions);
            var uow2 = new UnitOfWork(context2);
            var productReadRepo2 = new ProductReadRepository(context2);
            var productWriteRepo2 = new ProductWriteRepository(context2);

            var deleteProductUseCase = new DeleteProductUseCase(uow2, productReadRepo2, productWriteRepo2);

            await deleteProductUseCase.ExecuteAsync(1);
        });

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(async () => await createOrderTask);
        await deleteProductTask; // Esperar a que complete la eliminación
    }*/
   /*
   [Fact]
   public async Task CreateOrder_WithSufficientStock_ShouldSucceed()
   {
       // Arrange
       await using var context = new ApplicationDbContext(_dbOptions);
       var uow = new UnitOfWork(context);
       //var uow = new TestUnitOfWork(context); // Usar TestUnitOfWork
       var productReadRepo = new ProductReadRepository(context);
       var productWriteRepo = new ProductWriteRepository(context);
       var orderReadRepo = new OrderReadRepository(context);
       var orderWriteRepo = new OrderWriteRepository(context);

       var createOrderUseCase = new CreateOrderUseCase(
           uow, productReadRepo, productWriteRepo, orderReadRepo, orderWriteRepo);

       // Act
       var result = await createOrderUseCase.ExecuteAsync(
           new CreateOrderDto { ProductId = 1, Quantity = 1 }, "valid_key");

       // Assert
       result.Should().NotBeNull();
       result.ProductId.Should().Be(1);
       result.Quantity.Should().Be(1);
    
       // Verificar que el stock disminuyó
       var product = await productReadRepo.GetByIdAsync(1);
       product.Stock.Should().Be(2);
   }
   
   [Fact]
   public async Task CreateOrder_WhileDeleted_ShouldThrowNotFoundException()
   {
       // Arrange
       await using var contextForDelete = new ApplicationDbContext(_dbOptions);
       var uowForDelete = new UnitOfWork(contextForDelete);
       var productReadRepoForDelete = new ProductReadRepository(contextForDelete);
       var productWriteRepoForDelete = new ProductWriteRepository(contextForDelete);
    
       var deleteProductUseCase = new DeleteProductUseCase(uowForDelete, productReadRepoForDelete, productWriteRepoForDelete);

       // Act - Firstly delete the product
       await deleteProductUseCase.ExecuteAsync(1);

       // Arrange - Ahora intentar crear orden
       await using var contextForOrder = new ApplicationDbContext(_dbOptions);
       var uowForOrder = new UnitOfWork(contextForOrder);
       var productReadRepoForOrder = new ProductReadRepository(contextForOrder);
       var productWriteRepoForOrder = new ProductWriteRepository(contextForOrder);
       var orderReadRepoForOrder = new OrderReadRepository(contextForOrder);
       var orderWriteRepoForOrder = new OrderWriteRepository(contextForOrder);

       var createOrderUseCase = new CreateOrderUseCase(
           uowForOrder, productReadRepoForOrder, productWriteRepoForOrder, orderReadRepoForOrder, orderWriteRepoForOrder);

       // Act & Assert - It must be fauil becauso the product were deleted
       await Assert.ThrowsAsync<NotFoundException>(async () => 
           await createOrderUseCase.ExecuteAsync(
               new CreateOrderDto { ProductId = 1, Quantity = 1 }, "order_key"));
   }
   
  
   [Fact]
    public async Task DecreaseStock_Simultaneous_ShouldOnlyOneSucceed()
    {
        // Arrange
        var tasks = new List<Task<bool>>();
        var successCount = 0;
        var failureCount = 0;
        var lockObject = new object();

        // Act - Crear 5 órdenes simultáneas para el mismo producto (stock inicial = 3)
        for (int i = 0; i < 5; i++)
        {
            var task = Task.Run(async () =>
            {
                try
                {
                    // Cada task usa su propia conexión para verdadera concurrencia
                    var connection = new SqliteConnection("DataSource=:memory:");
                    await connection.OpenAsync();
                    
                    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                        .UseSqlite(connection)
                        .Options;

                    // Crear base de datos y datos para esta conexión
                    await using var context = new ApplicationDbContext(options);
                    await context.Database.EnsureCreatedAsync();
                    
                    // Re-crear el producto para esta conexión
                    var product = new Product
                    {
                        Id = 1,
                        Name = "Test Product",
                        Description = "Test Description", 
                        Price = 100.0m,
                        Stock = 3,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Version = 0
                    };
                    await context.Products.AddAsync(product);
                    await context.SaveChangesAsync();

                    var uow = new UnitOfWork(context);
                    var productWriteRepo = new ProductWriteRepository(context);

                    await uow.BeginTransactionAsync();
                    var result = await productWriteRepo.TryDecreaseStockAdminAsync(1, 1, CancellationToken.None);
                    
                    if (result)
                    {
                        await uow.CommitAsync();
                        lock (lockObject)
                        {
                            successCount++;
                        }
                    }
                    else
                    {
                        await uow.RollbackAsync();
                        lock (lockObject)
                        {
                            failureCount++;
                        }
                    }
                    
                    await connection.CloseAsync();
                    return result;
                }
                catch (Exception ex)
                {
                    lock (lockObject)
                    {
                        failureCount++;
                    }
                    return false;
                }
            });
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);

        // Assert - Con conexiones separadas, todas pueden tener éxito
        // pero el control de concurrencia en la lógica de negocio debería prevenir esto
        //successCount.Should().BeLessOrEqualTo(expectedValue);
        //Assert.True(actualValue <= expectedValue);
        //successCount.Should().BeLessOrEqualTo(3, "no más de 3 operaciones deberían tener éxito (stock inicial = 3)");
        Assert.True(successCount <= 3, $"no más de 3 operaciones deberían tener éxito (stock inicial = 3). Operaciones exitosas: {successCount}");
        Assert.True(failureCount >= 2, $"al menos 2 operaciones deberían fallar por control de concurrencia. Fallos actuales: {failureCount}"); 
        //failureCount.Should().BeGreaterOrEqualTo(2, "al menos 2 operaciones deberían fallar por control de concurrencia");
    } 
   
    [Fact]
    public async Task CreateOrder_Concurrent_WithStockLimit_ShouldRespectStock()
    {
        // Arrange - Stock inicial = 3
        var orderTasks = new List<Task<OrderDto>>();
        var successOrders = new List<OrderDto>();
        var failedOrders = new List<Exception>();
        var lockObject = new object();

        // Act - Crear 5 órdenes simultáneas
        for (int i = 0; i < 5; i++)
        {
            var taskId = i;
            var task = Task.Run(async () =>
            {
                try
                {
                    // Cada task usa su propia conexión
                    var connection = new SqliteConnection("DataSource=:memory:");
                    await connection.OpenAsync();
                    
                    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                        .UseSqlite(connection)
                        .Options;

                    await using var context = new ApplicationDbContext(options);
                    await context.Database.EnsureCreatedAsync();
                    
                    // Re-crear datos para esta conexión
                    var product = new Product
                    {
                        Id = 1,
                        Name = "Test Product",
                        Description = "Test Description",
                        Price = 100.0m,
                        Stock = 3,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Version = 0
                    };
                    await context.Products.AddAsync(product);
                    await context.SaveChangesAsync();

                    var uow = new UnitOfWork(context);
                    var productReadRepo = new ProductReadRepository(context);
                    var productWriteRepo = new ProductWriteRepository(context);
                    var orderReadRepo = new OrderReadRepository(context);
                    var orderWriteRepo = new OrderWriteRepository(context);

                    var createOrderUseCase = new CreateOrderUseCase(
                        uow, productReadRepo, productWriteRepo, orderReadRepo, orderWriteRepo);

                    var result = await createOrderUseCase.ExecuteAsync(
                        new CreateOrderDto { ProductId = 1, Quantity = 1 }, $"concurrent_key_{taskId}");

                    lock (lockObject)
                    {
                        successOrders.Add(result);
                    }
                    
                    await connection.CloseAsync();
                    return result;
                }
                catch (Exception ex)
                {
                    lock (lockObject)
                    {
                        failedOrders.Add(ex);
                    }
                    throw;
                }
            });
            orderTasks.Add(task);
        }

        // Esperar todas las tareas
        try
        {
            await Task.WhenAll(orderTasks);
        }
        catch
        {
            // Ignorar excepciones - ya las capturamos en failedOrders
        }

        // Assert - Con conexiones separadas, todas pueden tener éxito
        // pero el control de concurrencia real se prueba mejor con base de datos compartida
        successOrders.Should().NotBeEmpty("al menos una orden debería crearse exitosamente");
        
        // Verificar que no excedemos el stock lógico
        var totalQuantity = successOrders.Sum(o => o.Quantity);
        //totalQuantity.Should().BeLessOrEqualTo(3, "la cantidad total no debería exceder el stock inicial");
        Assert.True(totalQuantity <= 3, "la cantidad total no debería exceder el stock inicia");
    }
    
    [Fact]
    public async Task UpdateProduct_Concurrent_ShouldHandleVersionConflicts()
    {
        // Arrange
        var updateTasks = new List<Task<ProductDto?>>();
        var successUpdates = new List<ProductDto>();
        var failedUpdates = new List<Exception>();
        var lockObject = new object();

        // Act - Intentar actualizar el mismo producto concurrentemente
        for (int i = 0; i < 3; i++)
        {
            var taskId = i;
            var task = Task.Run(async () =>
            {
                try
                {
                    await using var context = new ApplicationDbContext(_dbOptions);
                    var uow = new UnitOfWork(context);
                    var productReadRepo = new ProductReadRepository(context);
                    var productWriteRepo = new ProductWriteRepository(context);

                    var updateProductUseCase = new UpdateProductUseCase(uow, productReadRepo, productWriteRepo, null);

                    var result = await updateProductUseCase.ExecuteAsync(
                        1, 
                        new UpdateProductDto 
                        { 
                            Name = $"Updated Product {taskId}", 
                            Description = $"Description {taskId}", 
                            Price = 100.0m + taskId 
                        }, 
                        CancellationToken.None);

                    lock (lockObject)
                    {
                        if (result != null)
                            successUpdates.Add(result);
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    lock (lockObject)
                    {
                        failedUpdates.Add(ex);
                    }
                    return null;
                }
            });
            updateTasks.Add(task);
        }

        await Task.WhenAll(updateTasks);

        // Assert - Solo una actualización debería tener éxito debido al control de versiones
        successUpdates.Should().HaveCount(1, "solo una actualización debería tener éxito debido al control de concurrencia");
        
        // Verificar que el producto fue actualizado
        await using var context = new ApplicationDbContext(_dbOptions);
        var productReadRepo = new ProductReadRepository(context);
        var product = await productReadRepo.GetByIdAsync(1);
        product.Should().NotBeNull();
        product.Version.Should().Be(1, "la versión debería incrementarse en 1");
    }
    
    [Fact]
    public async Task CreateOrder_And_UpdateProduct_Concurrent_ShouldNotInterfere()
    {
        // Arrange
        var createOrderTask = Task.Run(async () =>
        {
            await using var context = new ApplicationDbContext(_dbOptions);
            var uow = new UnitOfWork(context);
            var productReadRepo = new ProductReadRepository(context);
            var productWriteRepo = new ProductWriteRepository(context);
            var orderReadRepo = new OrderReadRepository(context);
            var orderWriteRepo = new OrderWriteRepository(context);

            var createOrderUseCase = new CreateOrderUseCase(
                uow, productReadRepo, productWriteRepo, orderReadRepo, orderWriteRepo);

            return await createOrderUseCase.ExecuteAsync(
                new CreateOrderDto { ProductId = 1, Quantity = 1 }, "concurrent_update_key");
        });

        var updateProductTask = Task.Run(async () =>
        {
            await using var context = new ApplicationDbContext(_dbOptions);
            var uow = new UnitOfWork(context);
            var productReadRepo = new ProductReadRepository(context);
            var productWriteRepo = new ProductWriteRepository(context);

            var updateProductUseCase = new UpdateProductUseCase(uow, productReadRepo, productWriteRepo, null);

            return await updateProductUseCase.ExecuteAsync(
                1, 
                new UpdateProductDto 
                { 
                    Name = "Concurrent Updated Product", 
                    Description = "Updated during order creation", 
                    Price = 150.0m 
                }, 
                CancellationToken.None);
        });

        // Act
        var results = new List<Order>();// await Task.WhenAll(createOrderTask, updateProductTask);
        var orderResult = results[0];
        var productResult = results[1];

        // Assert
        orderResult.Should().NotBeNull("la orden debería crearse exitosamente");
        productResult.Should().NotBeNull("el producto debería actualizarse exitosamente");
        
        orderResult.ProductId.Should().Be(1);
        orderResult.Quantity.Should().Be(1);
        
        //productResult.Name.Should().Be("Concurrent Updated Product");
        //productResult.Price.Should().Be(150.0m);

        // Verificar stock
        await using var context = new ApplicationDbContext(_dbOptions);
        var productReadRepo = new ProductReadRepository(context);
        var product = await productReadRepo.GetByIdAsync(1);
        product.Stock.Should().Be(2, "el stock debería disminuir en 1 por la orden creada");
    }
    
   /*[Fact]
   public async Task CreateOrder_WithSufficientStock_ShouldSucceed()
   {
       // Arrange - Reiniciar la base de datos para este test
       await using var context = new ApplicationDbContext(_dbOptions);
       var uow = new UnitOfWork(context);
       var productReadRepo = new ProductReadRepository(context);
       var productWriteRepo = new ProductWriteRepository(context);
       var orderReadRepo = new OrderReadRepository(context);
       var orderWriteRepo = new OrderWriteRepository(context);

       var createOrderUseCase = new CreateOrderUseCase(
           uow, productReadRepo, productWriteRepo, orderReadRepo, orderWriteRepo);

       // Act
       var result = await createOrderUseCase.ExecuteAsync(
           new CreateOrderDto { ProductId = 1, Quantity = 1 }, "valid_key");

       // Assert
       result.Should().NotBeNull();
       result.ProductId.Should().Be(1);
       result.Quantity.Should().Be(1);
        
       // Verificar que el stock disminuyó
       var product = await productReadRepo.GetByIdAsync(1);
       product.Stock.Should().Be(2);
   }*/
/*
    [Fact]
    public async Task Decrease_Stock_Simultaneous_ShouldOnlyOneSucceed()
    {
        // Arrange
        var tasks = new List<Task<bool>>();
        var successCount = 0;
        var failureCount = 0;
        var lockObject = new object();

        // Act - Crear 5 órdenes simultáneas para el mismo producto
        for (int i = 0; i < 5; i++)
        {
            var task = Task.Run(async () =>
            {
                try
                {
                    // Cada task usa su propio DbContext
                    await using var context = new ApplicationDbContext(_dbOptions);
                    var uow = new UnitOfWork(context);
                    var productWriteRepo = new ProductWriteRepository(context);

                    await uow.BeginTransactionAsync();
                    var result = await productWriteRepo.TryDecreaseStockAdminAsync(1, 1, CancellationToken.None);
                    
                    if (result)
                    {
                        await uow.CommitAsync();
                        lock (lockObject)
                        {
                            successCount++;
                        }
                    }
                    else
                    {
                        await uow.RollbackAsync();
                        lock (lockObject)
                        {
                            failureCount++;
                        }
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    lock (lockObject)
                    {
                        failureCount++;
                    }
                    return false;
                }
            });
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);

        // Assert
        successCount.Should().Be(1, "solo una operación debería tener éxito por concurrencia");
        failureCount.Should().Be(4, "cuatro operaciones deberían fallar por stock insuficiente");
    }  */ 
   
/*
    [Fact]
    public async Task CreateOrder_WhileDeleted_Exception()
    {
        var productReadRepo = new ProductReadRepository(_dbContext);
        var productWriteRepo = new ProductWriteRepository(_dbContext);
        var orderReadRepo = new OrderReadRepository(_dbContext);
        var orderWriteRepo = new OrderWriteRepository(_dbContext);

        var createOrderUseCase = new CreateOrderUseCase(
            _uow, productReadRepo, productWriteRepo, orderReadRepo, orderWriteRepo);

        var deleteProductUseCase = new DeleteProductUseCase(_uow, productReadRepo, productWriteRepo);

        // Act
        var orderTask = createOrderUseCase.ExecuteAsync(
            new CreateOrderDto { ProductId = 1, Quantity = 1 }, "order_key");

        var deleteTask = deleteProductUseCase.ExecuteAsync(1);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(() => orderTask);
    }

    [Fact]
    public async Task Decrease_Stock_Simultaneous()
    {
        var productWriteRepo = new ProductWriteRepository(_dbContext);
        var orderWriteRepo = new OrderWriteRepository(_dbContext);
        
        var tasks = new List<Task<bool>>();
        var successCount = 0;
        var failureCount = 0;

        // Act - Crear 5 órdenes simultáneas para el último producto en stock
        for (int i = 0; i < 5; i++)
        {
            var task = Task.Run(async () =>
            {
                try
                {
                    await using var context = new ApplicationDbContext(
                        new DbContextOptionsBuilder<ApplicationDbContext>()
                            .UseInMemoryDatabase("SameDatabase")
                            .Options);
                    
                    var uow = new UnitOfWork(context);
                    var productRepo = new ProductWriteRepository(context);
                    
                    await uow.BeginTransactionAsync();
                    var result = await productRepo.TryDecreaseStockAdminAsync(1, 1, CancellationToken.None);
                    if (result)
                    {
                        await uow.CommitAsync();
                        Interlocked.Increment(ref successCount);
                    }
                    else
                    {
                        await uow.RollbackAsync();
                        Interlocked.Increment(ref failureCount);
                    }
                    return result;
                }
                catch
                {
                    Interlocked.Increment(ref failureCount);
                    return false;
                }
            });
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);

        // Assert
        successCount.Should().Be(1);
        failureCount.Should().Be(4);
    }*/
}