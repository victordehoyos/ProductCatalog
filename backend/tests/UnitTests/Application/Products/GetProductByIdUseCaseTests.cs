using Moq;
using ProductCatalogAPI.Application.UseCases.Products;
using ProductCatalogAPI.Domain.Entities;
using ProductCatalogAPI.Domain.Interfaces.Read;
using ProductCatalogAPI.UnitTests.Shared.Builders;
using FluentAssertions;

namespace ProductCatalogAPI.UnitTests.Application.Products;

public class GetProductByIdUseCaseTests
{
    private readonly Mock<IProductReadRepository> _mockRepo;
    private readonly GetProductByIdUseCase _useCase;

    public GetProductByIdUseCaseTests()
    {
        _mockRepo = new Mock<IProductReadRepository>();
        _useCase = new GetProductByIdUseCase(_mockRepo.Object);
    }

    [Fact]
    public async Task ProductExists()
    {
        var product = ProductBuilder.BuildDefault();
        _mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);
        
        var result = await _useCase.ExecuteAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(product.Id);
        result.Name.Should().Be(product.Name);
        _mockRepo.Verify(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProductNotExists()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product)null);
        
        var result = await _useCase.ExecuteAsync(999);

        // Assert
        result.Should().BeNull();
    }
}