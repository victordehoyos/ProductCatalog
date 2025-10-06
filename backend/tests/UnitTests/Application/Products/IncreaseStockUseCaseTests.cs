using ProductCatalogAPI.Application.Ports;

namespace ProductCatalogAPI.UnitTests.Application.Products;

using Moq;
using ProductCatalogAPI.Application.UseCases.Products;
using ProductCatalogAPI.Domain.Interfaces.Write;
using Xunit;

public class IncreaseStockUseCaseTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IProductWriteRepository> _mockWriteRepository;
    private readonly IncreaseStockUseCase _useCase;

    public IncreaseStockUseCaseTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockWriteRepository = new Mock<IProductWriteRepository>();
        _useCase = new IncreaseStockUseCase(_mockUnitOfWork.Object, _mockWriteRepository.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidParameters_ShouldCompleteTransactionSuccessfully()
    {
        // Arrange
        var productId = 1;
        var quantity = 5;
        var cancellationToken = CancellationToken.None;

        _mockUnitOfWork
            .Setup(u => u.BeginTransactionAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        _mockWriteRepository
            .Setup(w => w.IncreaseStockAsync(productId, quantity, cancellationToken))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.CommitAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        await _useCase.ExecuteAsync(productId, quantity, cancellationToken);

        // Assert
        _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(cancellationToken), Times.Once);
        _mockWriteRepository.Verify(w => w.IncreaseStockAsync(productId, quantity, cancellationToken), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithZeroQuantity_ShouldCompleteTransaction()
    {
        // Arrange
        var productId = 1;
        var quantity = 0;
        var cancellationToken = CancellationToken.None;

        _mockUnitOfWork
            .Setup(u => u.BeginTransactionAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        _mockWriteRepository
            .Setup(w => w.IncreaseStockAsync(productId, quantity, cancellationToken))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.CommitAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        await _useCase.ExecuteAsync(productId, quantity, cancellationToken);

        // Assert
        _mockWriteRepository.Verify(w => w.IncreaseStockAsync(productId, quantity, cancellationToken), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNegativeQuantity_ShouldCompleteTransaction()
    {
        // Arrange
        var productId = 1;
        var quantity = -3; 
        var cancellationToken = CancellationToken.None;

        _mockUnitOfWork
            .Setup(u => u.BeginTransactionAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        _mockWriteRepository
            .Setup(w => w.IncreaseStockAsync(productId, quantity, cancellationToken))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.CommitAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        await _useCase.ExecuteAsync(productId, quantity, cancellationToken);

        // Assert
        _mockWriteRepository.Verify(w => w.IncreaseStockAsync(productId, quantity, cancellationToken), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithDifferentProductId_ShouldCallRepositoryWithCorrectId()
    {
        // Arrange
        var productId = 999;
        var quantity = 10;
        var cancellationToken = CancellationToken.None;

        _mockUnitOfWork
            .Setup(u => u.BeginTransactionAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        _mockWriteRepository
            .Setup(w => w.IncreaseStockAsync(productId, quantity, cancellationToken))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.CommitAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        await _useCase.ExecuteAsync(productId, quantity, cancellationToken);

        // Assert
        _mockWriteRepository.Verify(w => w.IncreaseStockAsync(productId, quantity, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithCancellationToken_ShouldPassTokenToAllOperations()
    {
        // Arrange
        var productId = 1;
        var quantity = 5;
        var cancellationToken = new CancellationTokenSource().Token;

        _mockUnitOfWork
            .Setup(u => u.BeginTransactionAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        _mockWriteRepository
            .Setup(w => w.IncreaseStockAsync(productId, quantity, cancellationToken))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.CommitAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        await _useCase.ExecuteAsync(productId, quantity, cancellationToken);

        // Assert
        _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(cancellationToken), Times.Once);
        _mockWriteRepository.Verify(w => w.IncreaseStockAsync(productId, quantity, cancellationToken), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRepositoryThrowsException_ShouldNotCommitTransaction()
    {
        // Arrange
        var productId = 1;
        var quantity = 5;
        var cancellationToken = CancellationToken.None;
        var expectedException = new Exception("Database error");

        _mockUnitOfWork
            .Setup(u => u.BeginTransactionAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        _mockWriteRepository
            .Setup(w => w.IncreaseStockAsync(productId, quantity, cancellationToken))
            .ThrowsAsync(expectedException);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => 
            _useCase.ExecuteAsync(productId, quantity, cancellationToken));

        _mockUnitOfWork.Verify(u => u.CommitAsync(cancellationToken), Times.Never);
    }
}