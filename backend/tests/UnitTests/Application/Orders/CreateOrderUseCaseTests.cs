using ProductCatalogAPI.Application.Ports;

namespace ProductCatalogAPI.UnitTests.Application.Orders;

using Moq;
using ProductCatalogAPI.Application.DTOs;
using ProductCatalogAPI.Application.Exceptions;
using ProductCatalogAPI.Application.UseCases.Orders;
using ProductCatalogAPI.Domain.Entities;
using ProductCatalogAPI.Domain.Interfaces.Read;
using ProductCatalogAPI.Domain.Interfaces.Write;
using Xunit;

public class CreateOrderUseCaseTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IProductReadRepository> _mockProductRead;
    private readonly Mock<IProductWriteRepository> _mockProductWrite;
    private readonly Mock<IOrderReadRepository> _mockOrderRead;
    private readonly Mock<IOrderWriteRepository> _mockOrderWrite;
    private readonly CreateOrderUseCase _useCase;

    public CreateOrderUseCaseTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockProductRead = new Mock<IProductReadRepository>();
        _mockProductWrite = new Mock<IProductWriteRepository>();
        _mockOrderRead = new Mock<IOrderReadRepository>();
        _mockOrderWrite = new Mock<IOrderWriteRepository>();
        _useCase = new CreateOrderUseCase(
            _mockUnitOfWork.Object,
            _mockProductRead.Object,
            _mockProductWrite.Object,
            _mockOrderRead.Object,
            _mockOrderWrite.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidData_ShouldCreateOrderAndReturnDto()
    {
        // Arrange
        var createOrderDto = new CreateOrderDto
        {
            ProductId = 1,
            Quantity = 2
        };
        var idempotencyKey = "test-key";
        var cancellationToken = CancellationToken.None;

        var product = new Product
        {
            Id = 1,
            Name = "Test Product",
            Price = 25.50m,
            Stock = 10
        };

        _mockOrderRead
            .Setup(r => r.GetByIdempotencyKeyAsync(idempotencyKey, cancellationToken))
            .ReturnsAsync((Order)null);

        _mockProductWrite
            .Setup(w => w.TryDecreaseStockAdminAsync(createOrderDto.ProductId, createOrderDto.Quantity, cancellationToken))
            .ReturnsAsync(true);

        _mockProductRead
            .Setup(r => r.GetByIdAsync(createOrderDto.ProductId, cancellationToken))
            .ReturnsAsync(product);

        _mockOrderWrite
            .Setup(w => w.AddAsync(It.IsAny<Order>(), cancellationToken))
            .ReturnsAsync((Order order, CancellationToken _) => order);

        _mockUnitOfWork
            .Setup(u => u.BeginTransactionAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.CommitAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(createOrderDto, idempotencyKey, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createOrderDto.ProductId, result.ProductId);
        Assert.Equal(createOrderDto.Quantity, result.Quantity);
        Assert.Equal(product.Price * createOrderDto.Quantity, result.Total);

        _mockProductWrite.Verify(w => w.TryDecreaseStockAdminAsync(createOrderDto.ProductId, createOrderDto.Quantity, cancellationToken), Times.Once);
        _mockOrderWrite.Verify(w => w.AddAsync(It.IsAny<Order>(), cancellationToken), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateIdempotencyKey_ShouldReturnExistingOrder()
    {
        // Arrange
        var createOrderDto = new CreateOrderDto
        {
            ProductId = 1,
            Quantity = 2
        };
        var idempotencyKey = "duplicate-key";
        var cancellationToken = CancellationToken.None;

        var existingOrder = new Order
        {
            Id = 100,
            ProductId = 1,
            Quantity = 2,
            Total = 51.00m,
            Date = DateTime.UtcNow.AddMinutes(-5)
        };

        _mockOrderRead
            .Setup(r => r.GetByIdempotencyKeyAsync(idempotencyKey, cancellationToken))
            .ReturnsAsync(existingOrder);

        // Act
        var result = await _useCase.ExecuteAsync(createOrderDto, idempotencyKey, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existingOrder.Id, result.Id);
        Assert.Equal(existingOrder.ProductId, result.ProductId);
        Assert.Equal(existingOrder.Quantity, result.Quantity);
        Assert.Equal(existingOrder.Total, result.Total);

        _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockProductWrite.Verify(w => w.TryDecreaseStockAdminAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockOrderWrite.Verify(w => w.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenInsufficientStock_ShouldRollbackAndThrowException()
    {
        // Arrange
        var createOrderDto = new CreateOrderDto
        {
            ProductId = 1,
            Quantity = 100
        };
        
        var product = new Product
        {
            Id = 1,
            Name = "Test Product",
            Price = 25.50m,
            Stock = 10
        };
        var idempotencyKey = "test-key";
        var cancellationToken = CancellationToken.None;

        _mockOrderRead
            .Setup(r => r.GetByIdempotencyKeyAsync(idempotencyKey, cancellationToken))
            .ReturnsAsync((Order)null);
        
        _mockProductRead
            .Setup(r => r.GetByIdAsync(createOrderDto.ProductId, cancellationToken))
            .ReturnsAsync(product);

        _mockOrderWrite
            .Setup(w => w.AddAsync(It.IsAny<Order>(), cancellationToken))
            .ReturnsAsync((Order order, CancellationToken _) => order);

        _mockProductWrite
            .Setup(w => w.TryDecreaseStockAdminAsync(createOrderDto.ProductId, createOrderDto.Quantity, cancellationToken))
            .ReturnsAsync(false);

        _mockUnitOfWork
            .Setup(u => u.BeginTransactionAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.RollbackAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        // Act & Assert
        await Assert.ThrowsAsync<InsufficientStockException>(() =>
            _useCase.ExecuteAsync(createOrderDto, idempotencyKey, cancellationToken));

        _mockUnitOfWork.Verify(u => u.RollbackAsync(cancellationToken), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockOrderWrite.Verify(w => w.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProductNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var createOrderDto = new CreateOrderDto
        {
            ProductId = 999,
            Quantity = 1
        };
        var idempotencyKey = "test-key";
        var cancellationToken = CancellationToken.None;

        _mockOrderRead
            .Setup(r => r.GetByIdempotencyKeyAsync(idempotencyKey, cancellationToken))
            .ReturnsAsync((Order)null);

        _mockProductWrite
            .Setup(w => w.TryDecreaseStockAdminAsync(createOrderDto.ProductId, createOrderDto.Quantity, cancellationToken))
            .ReturnsAsync(true);

        _mockProductRead
            .Setup(r => r.GetByIdAsync(createOrderDto.ProductId, cancellationToken))
            .ReturnsAsync((Product)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _useCase.ExecuteAsync(createOrderDto, idempotencyKey, cancellationToken));

        _mockUnitOfWork.Verify(u => u.RollbackAsync(cancellationToken), Times.Never);
        _mockOrderWrite.Verify(w => w.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
