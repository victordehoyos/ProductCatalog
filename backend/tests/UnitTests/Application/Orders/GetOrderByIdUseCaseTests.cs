namespace ProductCatalogAPI.UnitTests.Application.Orders;

using Moq;
using ProductCatalogAPI.Application.DTOs;
using ProductCatalogAPI.Application.UseCases.Orders;
using ProductCatalogAPI.Domain.Entities;
using ProductCatalogAPI.Domain.Interfaces.Read;
using Xunit;

public class GetOrderByIdUseCaseTests
{
    private readonly Mock<IOrderReadRepository> _mockOrderRead;
    private readonly GetOrderByIdUseCase _useCase;

    public GetOrderByIdUseCaseTests()
    {
        _mockOrderRead = new Mock<IOrderReadRepository>();
        _useCase = new GetOrderByIdUseCase(_mockOrderRead.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingOrder_ShouldReturnOrderDto()
    {
        // Arrange
        var orderId = 1;
        var order = new Order
        {
            Id = orderId,
            ProductId = 1,
            Quantity = 2,
            Total = 50.00m,
            Date = DateTime.UtcNow
        };

        var cancellationToken = CancellationToken.None;

        _mockOrderRead
            .Setup(r => r.GetByIdAsync(orderId, cancellationToken))
            .ReturnsAsync(order);

        // Act
        var result = await _useCase.ExecuteAsync(orderId, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(order.Id, result.Id);
        Assert.Equal(order.ProductId, result.ProductId);
        Assert.Equal(order.Quantity, result.Quantity);
        Assert.Equal(order.Total, result.Total);
        Assert.Equal(order.Date, result.Date);

        _mockOrderRead.Verify(r => r.GetByIdAsync(orderId, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistingOrder_ShouldReturnNull()
    {
        // Arrange
        var orderId = 999;
        var cancellationToken = CancellationToken.None;

        _mockOrderRead
            .Setup(r => r.GetByIdAsync(orderId, cancellationToken))
            .ReturnsAsync((Order)null);

        // Act
        var result = await _useCase.ExecuteAsync(orderId, cancellationToken);

        // Assert
        Assert.Null(result);
        _mockOrderRead.Verify(r => r.GetByIdAsync(orderId, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithDifferentOrderId_ShouldCallRepositoryWithCorrectId()
    {
        // Arrange
        var orderId = 123;
        var order = new Order
        {
            Id = orderId,
            ProductId = 1,
            Quantity = 1,
            Total = 25.50m,
            Date = DateTime.UtcNow
        };

        var cancellationToken = CancellationToken.None;

        _mockOrderRead
            .Setup(r => r.GetByIdAsync(orderId, cancellationToken))
            .ReturnsAsync(order);

        // Act
        var result = await _useCase.ExecuteAsync(orderId, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderId, result.Id);
        _mockOrderRead.Verify(r => r.GetByIdAsync(orderId, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithCancellationToken_ShouldPassTokenToRepository()
    {
        // Arrange
        var orderId = 1;
        var order = new Order
        {
            Id = orderId,
            ProductId = 1,
            Quantity = 3,
            Total = 75.00m,
            Date = DateTime.UtcNow
        };

        var cancellationToken = new CancellationTokenSource().Token;

        _mockOrderRead
            .Setup(r => r.GetByIdAsync(orderId, cancellationToken))
            .ReturnsAsync(order);

        // Act
        var result = await _useCase.ExecuteAsync(orderId, cancellationToken);

        // Assert
        Assert.NotNull(result);
        _mockOrderRead.Verify(r => r.GetByIdAsync(orderId, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithDefaultCancellationToken_ShouldUseDefaultToken()
    {
        // Arrange
        var orderId = 1;
        var order = new Order
        {
            Id = orderId,
            ProductId = 1,
            Quantity = 1,
            Total = 15.99m,
            Date = DateTime.UtcNow
        };

        _mockOrderRead
            .Setup(r => r.GetByIdAsync(orderId, default))
            .ReturnsAsync(order);

        // Act
        var result = await _useCase.ExecuteAsync(orderId); // Sin token

        // Assert
        Assert.NotNull(result);
        _mockOrderRead.Verify(r => r.GetByIdAsync(orderId, default), Times.Once);
    }
}
