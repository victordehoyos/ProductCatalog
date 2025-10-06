using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ProductCatalogAPI.Application.DTOs;
using ProductCatalogAPI.Application.UseCases.Orders;
using ProductCatalogAPI.Application.UseCases.Products;
using ProductCatalogAPI.Domain.Entities;
using ProductCatalogAPI.Domain.Interfaces.Read;
using ProductCatalogAPI.Domain.Interfaces.Write;
using ProductCatalogAPI.Infrastructure.Data;
using ProductCatalogAPI.Infrastructure.Persistence;
using ProductCatalogAPI.Infrastructure.Repositories.Read;
using ProductCatalogAPI.Infrastructure.Repositories.Write;
using ProductCatalogAPI.UnitTests.Shared.Builders;

namespace ProductCatalogAPI.UnitTests.Concurrency;

public class ProductConcurrencyTests //: IAsyncLifetime
{/*
    private ApplicationDbContext _dbContext;
    private ProductWriteRepository _writeRepository;
    private ProductReadRepository _readRepository;
    private UnitOfWork _uow;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"ConcurrencyTest_{Guid.NewGuid()}")
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _writeRepository = new ProductWriteRepository(_dbContext);
        _readRepository = new ProductReadRepository(_dbContext);
        _uow = new UnitOfWork(_dbContext);
        
        var product = ProductBuilder.BuildDefault();
        await _dbContext.Products.AddAsync(product);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }

    [Fact]
    public async Task Update_WhileDeleted_Exception()
    {
        var updateUseCase = new UpdateProductUseCase(
            _uow, _readRepository, _writeRepository, Mock.Of<ILogger<UpdateProductUseCase>>());
        
        var deleteUseCase = new DeleteProductUseCase(_uow, _readRepository, _writeRepository);

        // Act & Assert - Simulate concurrency
        var updateTask = updateUseCase.ExecuteAsync(1, new UpdateProductDto 
        { 
            Name = "Updated", 
            Price = 150 
        });

        var deleteTask = deleteUseCase.ExecuteAsync(1);

        // One o this operations must be fail because the concurrency
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => Task.WhenAll(updateTask, deleteTask));
    }

    [Fact]
    public async Task CreateOrder_WhthStockLimited()
    {
        var createOrderUseCase = new CreateOrderUseCase(
            _uow, _readRepository, _writeRepository, 
            Mock.Of<IOrderReadRepository>(), Mock.Of<IOrderWriteRepository>());

        var product = await _dbContext.Products.FindAsync(1);
        product.Stock = 5; // Stock limited

        // Act - Orders that consume complete stock
        var order1Task = createOrderUseCase.ExecuteAsync(
            new CreateOrderDto { ProductId = 1, Quantity = 5 }, 
            "key1");

        var order2Task = createOrderUseCase.ExecuteAsync(
            new CreateOrderDto { ProductId = 1, Quantity = 5 }, 
            "key2");

        var results = await Task.WhenAll(order1Task, order2Task);

        // Assert - Just one must be success
        results.Count(r => r != null).Should().Be(1);
    }*/
}