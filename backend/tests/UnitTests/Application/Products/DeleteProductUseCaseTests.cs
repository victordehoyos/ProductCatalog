using FluentAssertions;
using Moq;
using ProductCatalogAPI.Application.Ports;
using ProductCatalogAPI.Application.UseCases.Products;
using ProductCatalogAPI.Domain.Entities;
using ProductCatalogAPI.Domain.Interfaces.Read;
using ProductCatalogAPI.Domain.Interfaces.Write;
using ProductCatalogAPI.UnitTests.Shared.Builders;

namespace ProductCatalogAPI.UnitTests.Application.Products;

public class DeleteProductUseCaseTests
{
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly Mock<IProductReadRepository> _mockReadRepo;
    private readonly Mock<IProductWriteRepository> _mockWriteRepo;
    private readonly DeleteProductUseCase _useCase;

    public DeleteProductUseCaseTests()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockReadRepo = new Mock<IProductReadRepository>();
        _mockWriteRepo = new Mock<IProductWriteRepository>();

        _useCase = new DeleteProductUseCase(_mockUow.Object, _mockReadRepo.Object, _mockWriteRepo.Object);
    }

    [Fact]
    public async Task Delete_ProductExists()
    {
        var product = ProductBuilder.BuildDefault();
        _mockReadRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(product);
        _mockUow.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);
        _mockUow.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

        var result = await _useCase.ExecuteAsync(1);

        // Assert
        result.Should().BeTrue();
        _mockWriteRepo.Verify(w => w.DeleteAsync(product, It.IsAny<CancellationToken>()), Times.Once);
        _mockUow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_ProductNotExists()
    {
        _mockReadRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Product)null);

        var result = await _useCase.ExecuteAsync(999);

        // Assert
        result.Should().BeFalse();
        _mockWriteRepo.Verify(w => w.DeleteAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockUow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}