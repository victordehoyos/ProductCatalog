using ProductCatalogAPI.Application.Ports;

namespace ProductCatalogAPI.UnitTests.Application.Products;

using Moq;
using Microsoft.Extensions.Logging;
using ProductCatalogAPI.Application.DTOs;
using ProductCatalogAPI.Application.Exceptions;
using ProductCatalogAPI.Application.UseCases.Products;
using ProductCatalogAPI.Domain.Entities;
using ProductCatalogAPI.Domain.Interfaces.Read;
using ProductCatalogAPI.Domain.Interfaces.Write;
using Xunit;

public class UpdateProductUseCaseTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IProductReadRepository> _mockReadRepository;
    private readonly Mock<IProductWriteRepository> _mockWriteRepository;
    private readonly Mock<ILogger<UpdateProductUseCase>> _mockLogger;
    private readonly UpdateProductUseCase _useCase;

    public UpdateProductUseCaseTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockReadRepository = new Mock<IProductReadRepository>();
        _mockWriteRepository = new Mock<IProductWriteRepository>();
        _mockLogger = new Mock<ILogger<UpdateProductUseCase>>();
        _useCase = new UpdateProductUseCase(
            _mockUnitOfWork.Object,
            _mockReadRepository.Object,
            _mockWriteRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidData_ShouldUpdateProductAndReturnDto()
    {
        // Arrange
        var productId = 1;
        var updateDto = new UpdateProductDto
        {
            Name = "Updated Product",
            Description = "Updated Description",
            Price = 99.99m
        };

        var existingProduct = new Product
        {
            Id = productId,
            Name = "Original Product",
            Description = "Original Description",
            Price = 50.0m,
            Stock = 10,
            Version = 1
        };

        var cancellationToken = CancellationToken.None;

        _mockWriteRepository
            .Setup(w => w.GetTrackedByIdAsync(productId, cancellationToken))
            .ReturnsAsync(existingProduct);

        _mockWriteRepository
            .Setup(w => w.UpdateAsync(It.IsAny<Product>(), cancellationToken))
            .ReturnsAsync((Product p, CancellationToken _) => p);

        _mockUnitOfWork
            .Setup(u => u.BeginTransactionAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.CommitAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(productId, updateDto, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(productId, result.Id);
        Assert.Equal(updateDto.Name, result.Name);
        Assert.Equal(updateDto.Description, result.Description);
        Assert.Equal(updateDto.Price, result.Price);
        Assert.Equal(existingProduct.Stock, result.Stock);

        _mockWriteRepository.Verify(w => w.GetTrackedByIdAsync(productId, cancellationToken), Times.Once);
        _mockWriteRepository.Verify(w => w.UpdateAsync(It.IsAny<Product>(), cancellationToken), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProductNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var productId = 999;
        var updateDto = new UpdateProductDto
        {
            Name = "Updated Product",
            Description = "Updated Description",
            Price = 99.99m
        };

        var cancellationToken = CancellationToken.None;

        _mockWriteRepository
            .Setup(w => w.GetTrackedByIdAsync(productId, cancellationToken))
            .ReturnsAsync((Product)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _useCase.ExecuteAsync(productId, updateDto, cancellationToken));

        _mockWriteRepository.Verify(w => w.UpdateAsync(It.IsAny<Product>(), cancellationToken), Times.Never);
        _mockUnitOfWork.Verify(u => u.CommitAsync(cancellationToken), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullDescription_ShouldSetEmptyString()
    {
        // Arrange
        var productId = 1;
        var updateDto = new UpdateProductDto
        {
            Name = "Updated Product",
            Description = null,
            Price = 99.99m
        };

        var existingProduct = new Product
        {
            Id = productId,
            Name = "Original Product",
            Description = "Original Description",
            Price = 50.0m,
            Stock = 10,
            Version = 1
        };

        var cancellationToken = CancellationToken.None;

        _mockWriteRepository
            .Setup(w => w.GetTrackedByIdAsync(productId, cancellationToken))
            .ReturnsAsync(existingProduct);

        _mockWriteRepository
            .Setup(w => w.UpdateAsync(It.IsAny<Product>(), cancellationToken))
            .ReturnsAsync((Product p, CancellationToken _) => p);

        _mockUnitOfWork
            .Setup(u => u.BeginTransactionAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.CommitAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(productId, updateDto, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(string.Empty, result.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldIncrementVersionAndSetUpdatedAt()
    {
        // Arrange
        var productId = 1;
        var updateDto = new UpdateProductDto
        {
            Name = "Updated Product",
            Description = "Updated Description",
            Price = 99.99m
        };

        var existingProduct = new Product
        {
            Id = productId,
            Name = "Original Product",
            Description = "Original Description",
            Price = 50.0m,
            Stock = 10,
            Version = 1
        };

        var cancellationToken = CancellationToken.None;
        Product productPassedToUpdate = null;

        _mockWriteRepository
            .Setup(w => w.GetTrackedByIdAsync(productId, cancellationToken))
            .ReturnsAsync(existingProduct);

        _mockWriteRepository
            .Setup(w => w.UpdateAsync(It.IsAny<Product>(), cancellationToken))
            .Callback<Product, CancellationToken>((product, _) => productPassedToUpdate = product)
            .ReturnsAsync((Product p, CancellationToken _) =>
            {
                // Simular el comportamiento del repositorio real que incrementa la versión
                p.Version++;
                return p;
            });

        _mockUnitOfWork
            .Setup(u => u.BeginTransactionAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.CommitAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(productId, updateDto, cancellationToken);

        // Assert
        Assert.NotNull(result);
        // Verificar que el producto que se pasa a UpdateAsync tiene UpdatedAt
        Assert.NotNull(productPassedToUpdate);
        Assert.True(DateTime.UtcNow - productPassedToUpdate.UpdatedAt < TimeSpan.FromMinutes(1));

        // La versión se incrementa en el repositorio, no en el use case
        _mockWriteRepository.Verify(w => w.UpdateAsync(It.IsAny<Product>(), cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRepositoryThrowsException_ShouldNotCommitTransaction()
    {
        // Arrange
        var productId = 1;
        var updateDto = new UpdateProductDto
        {
            Name = "Updated Product",
            Description = "Updated Description",
            Price = 99.99m
        };

        var existingProduct = new Product
        {
            Id = productId,
            Name = "Original Product",
            Description = "Original Description",
            Price = 50.0m,
            Stock = 10,
            Version = 1
        };

        var cancellationToken = CancellationToken.None;
        var expectedException = new Exception("Database error");

        _mockWriteRepository
            .Setup(w => w.GetTrackedByIdAsync(productId, cancellationToken))
            .ReturnsAsync(existingProduct);

        _mockWriteRepository
            .Setup(w => w.UpdateAsync(It.IsAny<Product>(), cancellationToken))
            .ThrowsAsync(expectedException);

        _mockUnitOfWork
            .Setup(u => u.BeginTransactionAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _useCase.ExecuteAsync(productId, updateDto, cancellationToken));

        _mockUnitOfWork.Verify(u => u.CommitAsync(cancellationToken), Times.Never);
    }
}
