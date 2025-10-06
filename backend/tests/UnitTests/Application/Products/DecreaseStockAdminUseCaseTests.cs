using ProductCatalogAPI.Application.Ports;

namespace ProductCatalogAPI.UnitTests.Application.Products;

using Moq;
using ProductCatalogAPI.Application.UseCases.Products;
using ProductCatalogAPI.Domain.Interfaces.Write;
using Xunit;

public class DecreaseStockAdminUseCaseTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IProductWriteRepository> _mockWriteRepository;
    private readonly DecreaseStockAdminUseCase _useCase;

    public DecreaseStockAdminUseCaseTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockWriteRepository = new Mock<IProductWriteRepository>();
        _useCase = new DecreaseStockAdminUseCase(_mockUnitOfWork.Object, _mockWriteRepository.Object);
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
            .Setup(w => w.TryDecreaseStockAdminAsync(productId, quantity, cancellationToken))
            .ReturnsAsync(true);

        _mockUnitOfWork
            .Setup(u => u.CommitAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        await _useCase.ExecuteAsync(productId, quantity, cancellationToken);

        // Assert
        _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(cancellationToken), Times.Once);
        _mockWriteRepository.Verify(w => w.TryDecreaseStockAdminAsync(productId, quantity, cancellationToken), Times.Once);
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
            .Setup(w => w.TryDecreaseStockAdminAsync(productId, quantity, cancellationToken))
            .ReturnsAsync(true);

        _mockUnitOfWork
            .Setup(u => u.CommitAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        await _useCase.ExecuteAsync(productId, quantity, cancellationToken);

        // Assert
        _mockWriteRepository.Verify(w => w.TryDecreaseStockAdminAsync(productId, quantity, cancellationToken), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNegativeQuantity_ShouldCompleteTransaction()
    {
        // Arrange
        var productId = 1;
        var quantity = -10; // Negative quantity (might represent stock increase in some contexts)
        var cancellationToken = CancellationToken.None;

        _mockUnitOfWork
            .Setup(u => u.BeginTransactionAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        _mockWriteRepository
            .Setup(w => w.TryDecreaseStockAdminAsync(productId, quantity, cancellationToken))
            .ReturnsAsync(true);

        _mockUnitOfWork
            .Setup(u => u.CommitAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        await _useCase.ExecuteAsync(productId, quantity, cancellationToken);

        // Assert
        _mockWriteRepository.Verify(w => w.TryDecreaseStockAdminAsync(productId, quantity, cancellationToken), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithLargeQuantity_ShouldCompleteTransaction()
    {
        // Arrange
        var productId = 1;
        var quantity = 10000;
        var cancellationToken = CancellationToken.None;

        _mockUnitOfWork
            .Setup(u => u.BeginTransactionAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        _mockWriteRepository
            .Setup(w => w.TryDecreaseStockAdminAsync(productId, quantity, cancellationToken))
            .ReturnsAsync(true);

        _mockUnitOfWork
            .Setup(u => u.CommitAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        await _useCase.ExecuteAsync(productId, quantity, cancellationToken);

        // Assert
        _mockWriteRepository.Verify(w => w.TryDecreaseStockAdminAsync(productId, quantity, cancellationToken), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithDifferentProductIds_ShouldCallRepositoryWithCorrectId()
    {
        // Arrange
        var productIds = new[] { 1, 2, 3, 999 };
        var quantity = 5;
        var cancellationToken = CancellationToken.None;

        _mockUnitOfWork
            .Setup(u => u.BeginTransactionAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        _mockWriteRepository
            .Setup(w => w.TryDecreaseStockAdminAsync(It.IsAny<int>(), quantity, cancellationToken))
            .ReturnsAsync(true);

        _mockUnitOfWork
            .Setup(u => u.CommitAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        // Act & Assert
        foreach (var productId in productIds)
        {
            await _useCase.ExecuteAsync(productId, quantity, cancellationToken);

            _mockWriteRepository.Verify(w => w.TryDecreaseStockAdminAsync(productId, quantity, cancellationToken), Times.Once);
            _mockWriteRepository.Invocations.Clear(); // Clear invocations for next iteration
        }
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
            .Setup(w => w.TryDecreaseStockAdminAsync(productId, quantity, cancellationToken))
            .ReturnsAsync(true);

        _mockUnitOfWork
            .Setup(u => u.CommitAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        await _useCase.ExecuteAsync(productId, quantity, cancellationToken);

        // Assert
        _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(cancellationToken), Times.Once);
        _mockWriteRepository.Verify(w => w.TryDecreaseStockAdminAsync(productId, quantity, cancellationToken), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithDefaultCancellationToken_ShouldUseDefaultToken()
    {
        // Arrange
        var productId = 1;
        var quantity = 5;

        _mockUnitOfWork
            .Setup(u => u.BeginTransactionAsync(default))
            .Returns(Task.CompletedTask);

        _mockWriteRepository
            .Setup(w => w.TryDecreaseStockAdminAsync(productId, quantity, default))
            .ReturnsAsync(true);

        _mockUnitOfWork
            .Setup(u => u.CommitAsync(default))
            .Returns(Task.CompletedTask);

        // Act
        await _useCase.ExecuteAsync(productId, quantity); // No token provided, should use default

        // Assert
        _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(default), Times.Once);
        _mockWriteRepository.Verify(w => w.TryDecreaseStockAdminAsync(productId, quantity, default), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(default), Times.Once);
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
            .Setup(w => w.TryDecreaseStockAdminAsync(productId, quantity, cancellationToken))
            .ThrowsAsync(expectedException);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _useCase.ExecuteAsync(productId, quantity, cancellationToken));

        _mockUnitOfWork.Verify(u => u.CommitAsync(cancellationToken), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenBeginTransactionThrowsException_ShouldNotCallRepositoryOrCommit()
    {
        // Arrange
        var productId = 1;
        var quantity = 5;
        var cancellationToken = CancellationToken.None;
        var expectedException = new Exception("Transaction error");

        _mockUnitOfWork
            .Setup(u => u.BeginTransactionAsync(cancellationToken))
            .ThrowsAsync(expectedException);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _useCase.ExecuteAsync(productId, quantity, cancellationToken));

        _mockWriteRepository.Verify(w => w.TryDecreaseStockAdminAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallOperationsInCorrectOrder()
    {
        // Arrange
        var productId = 1;
        var quantity = 5;
        var cancellationToken = CancellationToken.None;
        var callOrder = new List<string>();

        _mockUnitOfWork
            .Setup(u => u.BeginTransactionAsync(cancellationToken))
            .Callback(() => callOrder.Add("BeginTransaction"))
            .Returns(Task.CompletedTask);

        _mockWriteRepository
            .Setup(w => w.TryDecreaseStockAdminAsync(productId, quantity, cancellationToken))
            .Callback(() => callOrder.Add("TryDecreaseStockAdminAsync"))
            .ReturnsAsync(true);

        _mockUnitOfWork
            .Setup(u => u.CommitAsync(cancellationToken))
            .Callback(() => callOrder.Add("Commit"))
            .Returns(Task.CompletedTask);

        // Act
        await _useCase.ExecuteAsync(productId, quantity, cancellationToken);

        // Assert
        Assert.Equal(3, callOrder.Count);
        Assert.Equal("BeginTransaction", callOrder[0]);
        Assert.Equal("TryDecreaseStockAdminAsync", callOrder[1]);
        Assert.Equal("Commit", callOrder[2]);
    }
}