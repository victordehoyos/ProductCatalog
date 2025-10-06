namespace ProductCatalogAPI.UnitTests.Application.Orders;

using Moq;
using ProductCatalogAPI.Application.DTOs;
using ProductCatalogAPI.Application.UseCases.Orders;
using ProductCatalogAPI.Domain.Entities;
using ProductCatalogAPI.Domain.Interfaces.Read;
using Xunit;

public class GetAllOrdersUserCaseTests
{
    private readonly Mock<IOrderReadRepository> _mockOrderRead;
    private readonly GetAllOrdersUserCase _useCase;

    public GetAllOrdersUserCaseTests()
    {
        _mockOrderRead = new Mock<IOrderReadRepository>();
        _useCase = new GetAllOrdersUserCase(_mockOrderRead.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithOrders_ShouldReturnMappedOrderDtos()
    {
        // Arrange
        var orders = new List<Order>
        {
            new Order 
            { 
                Id = 1, 
                ProductId = 1, 
                Quantity = 2, 
                Total = 50.00m, 
                Date = DateTime.UtcNow.AddDays(-1)
            },
            new Order 
            { 
                Id = 2, 
                ProductId = 2, 
                Quantity = 1, 
                Total = 25.50m, 
                Date = DateTime.UtcNow 
            }
        };

        var cancellationToken = CancellationToken.None;

        _mockOrderRead
            .Setup(r => r.GetAllAsync(cancellationToken))
            .ReturnsAsync(orders);

        // Act
        var result = await _useCase.ExecuteAsync(cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        var firstOrder = result[0];
        Assert.Equal(orders[0].Id, firstOrder.Id);
        Assert.Equal(orders[0].ProductId, firstOrder.ProductId);
        Assert.Equal(orders[0].Quantity, firstOrder.Quantity);
        Assert.Equal(orders[0].Total, firstOrder.Total);
        Assert.Equal(orders[0].Date, firstOrder.Date);

        var secondOrder = result[1];
        Assert.Equal(orders[1].Id, secondOrder.Id);
        Assert.Equal(orders[1].ProductId, secondOrder.ProductId);
        Assert.Equal(orders[1].Quantity, secondOrder.Quantity);
        Assert.Equal(orders[1].Total, secondOrder.Total);
        Assert.Equal(orders[1].Date, secondOrder.Date);

        _mockOrderRead.Verify(r => r.GetAllAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        var orders = new List<Order>();
        var cancellationToken = CancellationToken.None;

        _mockOrderRead
            .Setup(r => r.GetAllAsync(cancellationToken))
            .ReturnsAsync(orders);

        // Act
        var result = await _useCase.ExecuteAsync(cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockOrderRead.Verify(r => r.GetAllAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithCancellationToken_ShouldPassTokenToRepository()
    {
        // Arrange
        var orders = new List<Order>
        {
            new Order 
            { 
                Id = 1, 
                ProductId = 1, 
                Quantity = 3, 
                Total = 75.00m, 
                Date = DateTime.UtcNow 
            }
        };

        var cancellationToken = new CancellationTokenSource().Token;

        _mockOrderRead
            .Setup(r => r.GetAllAsync(cancellationToken))
            .ReturnsAsync(orders);

        // Act
        var result = await _useCase.ExecuteAsync(cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        _mockOrderRead.Verify(r => r.GetAllAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithDefaultCancellationToken_ShouldUseDefaultToken()
    {
        // Arrange
        var orders = new List<Order>
        {
            new Order 
            { 
                Id = 1, 
                ProductId = 1, 
                Quantity = 1, 
                Total = 15.99m, 
                Date = DateTime.UtcNow 
            }
        };

        _mockOrderRead
            .Setup(r => r.GetAllAsync(default))
            .ReturnsAsync(orders);

        // Act
        var result = await _useCase.ExecuteAsync(); // Sin token

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        _mockOrderRead.Verify(r => r.GetAllAsync(default), Times.Once);
    }
}
