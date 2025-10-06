namespace ProductCatalogAPI.UnitTests.Application.Products;

using Moq;
using ProductCatalogAPI.Application.DTOs;
using ProductCatalogAPI.Application.UseCases.Products;
using ProductCatalogAPI.Domain.Entities;
using ProductCatalogAPI.Domain.Interfaces.Read;
using Xunit;

public class GetAllProductsUseCaseTests
{
    private readonly Mock<IProductReadRepository> _mockReadRepository;
    private readonly GetAllProductsUseCase _useCase;

    public GetAllProductsUseCaseTests()
    {
        _mockReadRepository = new Mock<IProductReadRepository>();
        _useCase = new GetAllProductsUseCase(_mockReadRepository.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithProducts_ShouldReturnMappedProductDtos()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product 
            { 
                Id = 1, 
                Name = "Product 1", 
                Description = "Description 1", 
                Price = 10.99m, 
                Stock = 5 
            },
            new Product 
            { 
                Id = 2, 
                Name = "Product 2", 
                Description = "Description 2", 
                Price = 20.50m, 
                Stock = 10 
            }
        };

        var cancellationToken = CancellationToken.None;

        _mockReadRepository
            .Setup(r => r.GetAllAsync(cancellationToken))
            .ReturnsAsync(products);

        // Act
        var result = await _useCase.ExecuteAsync(cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        var firstProduct = result[0];
        Assert.Equal(products[0].Id, firstProduct.Id);
        Assert.Equal(products[0].Name, firstProduct.Name);
        Assert.Equal(products[0].Description, firstProduct.Description);
        Assert.Equal(products[0].Price, firstProduct.Price);
        Assert.Equal(products[0].Stock, firstProduct.Stock);

        var secondProduct = result[1];
        Assert.Equal(products[1].Id, secondProduct.Id);
        Assert.Equal(products[1].Name, secondProduct.Name);
        Assert.Equal(products[1].Description, secondProduct.Description);
        Assert.Equal(products[1].Price, secondProduct.Price);
        Assert.Equal(products[1].Stock, secondProduct.Stock);

        _mockReadRepository.Verify(r => r.GetAllAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        var products = new List<Product>();
        var cancellationToken = CancellationToken.None;

        _mockReadRepository
            .Setup(r => r.GetAllAsync(cancellationToken))
            .ReturnsAsync(products);

        // Act
        var result = await _useCase.ExecuteAsync(cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockReadRepository.Verify(r => r.GetAllAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithCancellationToken_ShouldPassTokenToRepository()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product 
            { 
                Id = 1, 
                Name = "Product 1", 
                Description = "Description 1", 
                Price = 15.99m, 
                Stock = 3 
            }
        };

        var cancellationToken = new CancellationTokenSource().Token;

        _mockReadRepository
            .Setup(r => r.GetAllAsync(cancellationToken))
            .ReturnsAsync(products);

        // Act
        var result = await _useCase.ExecuteAsync(cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        _mockReadRepository.Verify(r => r.GetAllAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithDefaultCancellationToken_ShouldUseDefaultToken()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product 
            { 
                Id = 1, 
                Name = "Product 1", 
                Description = "Test", 
                Price = 9.99m, 
                Stock = 1 
            }
        };

        _mockReadRepository
            .Setup(r => r.GetAllAsync(default))
            .ReturnsAsync(products);

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        _mockReadRepository.Verify(r => r.GetAllAsync(default), Times.Once);
    }
}
