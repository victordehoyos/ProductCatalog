using Moq;
using ProductCatalogAPI.Application.DTOs;
using ProductCatalogAPI.Application.Ports;
using ProductCatalogAPI.Application.UseCases.Products;
using ProductCatalogAPI.Domain.Entities;
using ProductCatalogAPI.Domain.Interfaces.Write;

namespace ProductCatalogAPI.UnitTests.Application.Products;

public class CreateProductUseCaseTests
{
    private readonly Mock<IProductWriteRepository> _mockRepo;
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly CreateProductUseCase _useCase;

    public CreateProductUseCaseTests()
    {
        _mockRepo = new Mock<IProductWriteRepository>();
        _mockUow = new Mock<IUnitOfWork>();
        _useCase = new CreateProductUseCase(_mockUow.Object, _mockRepo.Object);
    }

    [Fact]
    public async Task Create_WithValidDto()
    {
        // Arrange
        var createProductDto = new CreateProductDto
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            Stock = 10
        };

        var cancellationToken = CancellationToken.None;
        Product capturedProduct = null;
        var expectedId = 1;

        _mockUow
            .Setup(u => u.BeginTransactionAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        _mockRepo
            .Setup(w => w.AddAsync(It.IsAny<Product>(), cancellationToken))
            .Callback<Product, CancellationToken>((product, _) =>
            {
                capturedProduct = product;
                var idProperty = product.GetType().GetProperty("Id");
                if (idProperty != null && idProperty.PropertyType == typeof(int))
                {
                    idProperty.SetValue(product, expectedId);
                }
            })
            .ReturnsAsync((Product product, CancellationToken _) => product);

        _mockUow
            .Setup(u => u.CommitAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(createProductDto, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedId, result.Id);

        Assert.Equal(createProductDto.Name, result.Name);
        Assert.Equal(createProductDto.Description, result.Description);
        Assert.Equal(createProductDto.Price, result.Price);
        Assert.Equal(createProductDto.Stock, result.Stock);

        // Verify product entity was created correctly
        Assert.NotNull(capturedProduct);
        Assert.Equal(createProductDto.Name, capturedProduct.Name);
        Assert.Equal(createProductDto.Description, capturedProduct.Description);
        Assert.Equal(createProductDto.Price, capturedProduct.Price);
        Assert.Equal(createProductDto.Stock, capturedProduct.Stock);
        Assert.Equal(0, capturedProduct.Version);
        Assert.True(DateTime.UtcNow - capturedProduct.CreatedAt < TimeSpan.FromMinutes(1));

        // Verify interactions
        _mockUow.Verify(u => u.BeginTransactionAsync(cancellationToken), Times.Once);
        _mockRepo.Verify(w => w.AddAsync(It.IsAny<Product>(), cancellationToken), Times.Once);
        _mockUow.Verify(u => u.CommitAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullDescription_ShouldSetEmptyString()
    {
        // Arrange
        var createProductDto = new CreateProductDto
        {
            Name = "Test Product",
            Description = null,
            Price = 50.0m,
            Stock = 5
        };

        var cancellationToken = CancellationToken.None;
        Product capturedProduct = null;

        _mockUow
            .Setup(u => u.BeginTransactionAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        _mockRepo
            .Setup(w => w.AddAsync(It.IsAny<Product>(), cancellationToken))
            .Callback<Product, CancellationToken>((product, _) =>
            {
                capturedProduct = product;
                // Asignar ID de tipo int
                var idProperty = product.GetType().GetProperty("Id");
                if (idProperty != null && idProperty.PropertyType == typeof(int))
                {
                    idProperty.SetValue(product, 1);
                }
            })
            .ReturnsAsync((Product product, CancellationToken _) => product);

        _mockUow
            .Setup(u => u.CommitAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(createProductDto, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(string.Empty, result.Description);
        Assert.NotNull(capturedProduct);
        Assert.Equal(string.Empty, capturedProduct.Description);
    }
    
    [Fact]
    public async Task ExecuteAsync_WithEmptyDescription_ShouldPreserveEmptyString()
    {
        // Arrange
        var createProductDto = new CreateProductDto
        {
            Name = "Test Product",
            Description = "",
            Price = 50.0m,
            Stock = 5
        };

        var cancellationToken = CancellationToken.None;
        Product capturedProduct = null;

        _mockUow
            .Setup(u => u.BeginTransactionAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        _mockRepo
            .Setup(w => w.AddAsync(It.IsAny<Product>(), cancellationToken))
            .Callback<Product, CancellationToken>((product, _) => 
            {
                capturedProduct = product;
                var idProperty = product.GetType().GetProperty("Id");
                if (idProperty != null && idProperty.PropertyType == typeof(int))
                {
                    idProperty.SetValue(product, 1);
                }
            })
            .ReturnsAsync((Product product, CancellationToken _) => product);

        _mockUow
            .Setup(u => u.CommitAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(createProductDto, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(string.Empty, result.Description);
        Assert.NotNull(capturedProduct);
        Assert.Equal(string.Empty, capturedProduct.Description);
    }
    
    [Fact]
    public async Task ExecuteAsync_WithZeroPriceAndStock_ShouldCreateProductSuccessfully()
    {
        // Arrange
        var createProductDto = new CreateProductDto
        {
            Name = "Free Product",
            Description = "Free item",
            Price = 0m,
            Stock = 0
        };

        var cancellationToken = CancellationToken.None;

        _mockUow
            .Setup(u => u.BeginTransactionAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        _mockRepo
            .Setup(w => w.AddAsync(It.IsAny<Product>(), cancellationToken))
            .ReturnsAsync((Product product, CancellationToken _) => 
            {
                var idProperty = product.GetType().GetProperty("Id");
                if (idProperty != null && idProperty.PropertyType == typeof(int))
                {
                    idProperty.SetValue(product, 1);
                }
                return product;
            });

        _mockUow
            .Setup(u => u.CommitAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(createProductDto, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0m, result.Price);
        Assert.Equal(0, result.Stock);
    }
    
    [Fact]
    public async Task ExecuteAsync_WithCancellationToken_ShouldPassTokenToAllOperations()
    {
        // Arrange
        var createProductDto = new CreateProductDto
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 100m,
            Stock = 20
        };

        var cancellationToken = new CancellationTokenSource().Token;

        _mockUow
            .Setup(u => u.BeginTransactionAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        _mockRepo
            .Setup(w => w.AddAsync(It.IsAny<Product>(), cancellationToken))
            .ReturnsAsync((Product product, CancellationToken _) => 
            {
                var idProperty = product.GetType().GetProperty("Id");
                if (idProperty != null && idProperty.PropertyType == typeof(int))
                {
                    idProperty.SetValue(product, 1);
                }
                return product;
            });

        _mockUow
            .Setup(u => u.CommitAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        await _useCase.ExecuteAsync(createProductDto, cancellationToken);

        // Assert
        _mockUow.Verify(u => u.BeginTransactionAsync(cancellationToken), Times.Once);
        _mockRepo.Verify(w => w.AddAsync(It.IsAny<Product>(), cancellationToken), Times.Once);
        _mockUow.Verify(u => u.CommitAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSetCorrectInitialValues()
    {
        // Arrange
        var createProductDto = new CreateProductDto
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 75.5m,
            Stock = 15
        };

        var cancellationToken = CancellationToken.None;
        Product capturedProduct = null;

        _mockUow
            .Setup(u => u.BeginTransactionAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        _mockRepo
            .Setup(w => w.AddAsync(It.IsAny<Product>(), cancellationToken))
            .Callback<Product, CancellationToken>((product, _) => capturedProduct = product)
            .ReturnsAsync((Product product, CancellationToken _) => 
            {
                var idProperty = product.GetType().GetProperty("Id");
                if (idProperty != null && idProperty.PropertyType == typeof(int))
                {
                    idProperty.SetValue(product, 1);
                }
                return product;
            });

        _mockUow
            .Setup(u => u.CommitAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        await _useCase.ExecuteAsync(createProductDto, cancellationToken);

        // Assert
        Assert.NotNull(capturedProduct);
        Assert.Equal(0, capturedProduct.Version);
        Assert.True(DateTime.UtcNow - capturedProduct.CreatedAt < TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRepositoryThrowsException_ShouldNotCommitTransaction()
    {
        // Arrange
        var createProductDto = new CreateProductDto
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 100m,
            Stock = 20
        };

        var cancellationToken = CancellationToken.None;
        var expectedException = new Exception("Database error");

        _mockUow
            .Setup(u => u.BeginTransactionAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        _mockRepo
            .Setup(w => w.AddAsync(It.IsAny<Product>(), cancellationToken))
            .ThrowsAsync(expectedException);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => 
            _useCase.ExecuteAsync(createProductDto, cancellationToken));

        _mockUow.Verify(u => u.CommitAsync(cancellationToken), Times.Never);
    }
}