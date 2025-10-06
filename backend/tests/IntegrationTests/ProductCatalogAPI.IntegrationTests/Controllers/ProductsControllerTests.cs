using Microsoft.AspNetCore.TestHost;
using ProductCatalogAPI.Application.Mappings;
using ProductCatalogAPI.Application.Ports;
using ProductCatalogAPI.Domain.Interfaces.Read;
using ProductCatalogAPI.Domain.Interfaces.Write;

namespace ProductCatalogAPI.IntegrationTests.Controllers;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ProductCatalogAPI.Application.DTOs;
using ProductCatalogAPI.Application.Exceptions;
using ProductCatalogAPI.Application.UseCases.Products;
using ProductCatalogAPI.Domain.Entities;
using ProductCatalogAPI.Infrastructure.Services;
using System.Net;
using System.Text;
using System.Text.Json;

public class ProductsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly Mock<GetAllProductsUseCase> _mockGetAllUseCase;
    private readonly Mock<GetProductByIdUseCase> _mockGetByIdUseCase;
    private readonly Mock<CreateProductUseCase> _mockCreateUseCase;
    private readonly Mock<UpdateProductUseCase> _mockUpdateUseCase;
    private readonly Mock<DeleteProductUseCase> _mockDeleteUseCase;
    private readonly Mock<IncreaseStockUseCase> _mockIncreaseStockUseCase;
    
    private readonly Mock<IOrderReadRepository> _mockOrderReadRepository;
    private readonly Mock<IOrderWriteRepository> _mockOrderWriteRepository;
    private readonly Mock<IProductReadRepository> _mockProductReadRepository;
    private readonly Mock<IProductWriteRepository> _mockProductWriteRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;

    public ProductsControllerTests()
    {
        _mockJwtService = new Mock<IJwtService>();
        _mockOrderReadRepository = new Mock<IOrderReadRepository>();
        _mockOrderWriteRepository = new Mock<IOrderWriteRepository>();
        _mockProductReadRepository = new Mock<IProductReadRepository>();
        _mockProductWriteRepository = new Mock<IProductWriteRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddScoped<IJwtService>(_ => _mockJwtService.Object);
                    services.AddScoped<IOrderReadRepository>(_ => _mockOrderReadRepository.Object);
                    services.AddScoped<IOrderWriteRepository>(_ => _mockOrderWriteRepository.Object);
                    services.AddScoped<IProductReadRepository>(_ => _mockProductReadRepository.Object);
                    services.AddScoped<IProductWriteRepository>(_ => _mockProductWriteRepository.Object);
                    services.AddScoped<IUnitOfWork>(_ => _mockUnitOfWork.Object);
                });
            });
    }

    private HttpClient CreateClient()
    {
        return _factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_ReturnsOkWithProductsAndAuthorizationHeader()
    {
        // Arrange
        var expectedToken = "test-jwt-token";
        var expectedProducts = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", Description = "Description 1", Price = 10.99m, Stock = 5 },
            new Product { Id = 2, Name = "Product 2", Description = "Description 2", Price = 20.50m, Stock = 10 }
        };

        _mockJwtService.Setup(x => x.GenerateToken()).Returns(expectedToken);
        _mockProductReadRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProducts);

        using var client = CreateClient();

        // Act
        var response = await client.GetAsync("/api/products");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        Assert.True(response.Headers.Contains("Authorization"));
        var authHeader = response.Headers.GetValues("Authorization").First();
        Assert.Equal($"Bearer {expectedToken}", authHeader);

        var content = await response.Content.ReadAsStringAsync();
        var products = JsonSerializer.Deserialize<List<ProductDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(products);
        Assert.Equal(2, products.Count);
        Assert.Equal(expectedProducts[0].Id, products[0].Id);
    }

    [Fact]
    public async Task GetProduct_WithValidId_ReturnsProduct()
    {
        // Arrange
        var productId = 1;
        var expectedProduct = new ProductDto 
        { 
            Id = productId, 
            Name = "Test Product", 
            Description = "Test Description", 
            Price = 99.99m, 
            Stock = 10 
        };
        var p = ProductMapper.ToEntity(expectedProduct);
        
        _mockJwtService.Setup(x => x.GenerateToken()).Returns("test-token");
        _mockProductReadRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(p);

        using var client = CreateClient();

        // Act
        var response = await client.GetAsync($"/api/products/{productId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var product = JsonSerializer.Deserialize<ProductDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(product);
        Assert.Equal(expectedProduct.Id, product.Id);
        Assert.Equal(expectedProduct.Name, product.Name);
        Assert.Equal(expectedProduct.Price, product.Price);
    }

    [Fact]
    public async Task GetProduct_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        var productId = 999;
        
        _mockJwtService.Setup(x => x.GenerateToken()).Returns("test-token");
        _mockProductReadRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        using var client = CreateClient();

        // Act
        var response = await client.GetAsync($"/api/products/{productId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ReturnsCreated()
    {
        // Arrange
        var createProductDto = new CreateProductDto 
        { 
            Name = "New Product", 
            Description = "New Description", 
            Price = 49.99m, 
            Stock = 5 
        };
        var expectedProduct = new ProductDto 
        { 
            Id = 1, 
            Name = "New Product", 
            Description = "New Description", 
            Price = 49.99m, 
            Stock = 5 
        };
        
        _mockJwtService.Setup(x => x.GenerateToken()).Returns("test-token");
        _mockProductWriteRepository.Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((product, ct) =>
            {
                product.Id = 1;
            }).ReturnsAsync((Product product, CancellationToken _) => product);
        _mockUnitOfWork.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        using var client = CreateClient();

        var jsonContent = JsonSerializer.Serialize(createProductDto);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/products", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        Assert.NotNull(response.Headers.Location);
        Assert.Contains($"/api/products/{expectedProduct.Id}", response.Headers.Location.ToString().ToLower());

        var responseContent = await response.Content.ReadAsStringAsync();
        var product = JsonSerializer.Deserialize<ProductDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(product);
        Assert.Equal(expectedProduct.Id, product.Id);
        Assert.Equal(expectedProduct.Name, product.Name);
    }

    [Fact]
    public async Task CreateProduct_WhenUseCaseThrowsException_ReturnsBadRequest()
    {
        // Arrange
        var createProductDto = new CreateProductDto 
        { 
            Name = "New Product", 
            Price = 49.99m, 
            Stock = 5 
        };
        var errorMessage = "Invalid product data";
        
        _mockJwtService.Setup(x => x.GenerateToken()).Returns("test-token");
        _mockProductWriteRepository.Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(errorMessage));
        _mockUnitOfWork.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        using var client = CreateClient();

        var jsonContent = JsonSerializer.Serialize(createProductDto);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/products", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains(errorMessage, responseContent);
    }

    [Fact]
    public async Task UpdateProduct_WithValidData_ReturnsUpdatedProduct()
    {
        // Arrange
        var productId = 1;
        var updateProductDto = new UpdateProductDto 
        { 
            Name = "Updated Product", 
            Description = "Updated Description", 
            Price = 79.99m
        };
        var existingProduct = new Product 
        { 
            Id = productId, 
            Name = "Updated Product", 
            Description = "Updated Description", 
            Price = 79.99m, 
            Stock = 10,
            Version = 1
        };
        
        _mockJwtService.Setup(x => x.GenerateToken()).Returns("test-token");
        _mockProductWriteRepository.Setup(x => x.GetTrackedByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);
        _mockProductWriteRepository.Setup(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product product, CancellationToken _) => product);
        _mockUnitOfWork.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        using var client = CreateClient();

        var jsonContent = JsonSerializer.Serialize(updateProductDto);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PutAsync($"/api/products/{productId}", content);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var product = JsonSerializer.Deserialize<ProductDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(product);
        Assert.Equal(updateProductDto.Name, product.Name);
        Assert.Equal(updateProductDto.Description, product.Description);
        Assert.Equal(updateProductDto.Price, product.Price);
        Assert.Equal(existingProduct.Stock, product.Stock);
    }
   
    [Fact]
    public async Task UpdateProduct_WhenProductNotFound_ReturnsBadRequest()
    {
        // Arrange
        var productId = 999;
        var updateProductDto = new UpdateProductDto 
        { 
            Name = "Updated Product", 
            Price = 79.99m 
        };
    
        _mockJwtService.Setup(x => x.GenerateToken()).Returns("test-token");
    
        // Producto no encontrado - el use case lanzará NotFoundException
        _mockProductWriteRepository.Setup(x => x.GetTrackedByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        using var client = CreateClient();

        var jsonContent = JsonSerializer.Serialize(updateProductDto);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PutAsync($"/api/products/{productId}", content);

        // Assert - Cambiar a BadRequest
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    
        // Opcional: Verificar el mensaje de error
        var contentResponse = await response.Content.ReadAsStringAsync();
        Assert.Contains("Producto no existe o fue eliminado", contentResponse);
    }
    
    [Fact]
    public async Task DeleteProduct_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var productId = 1;
        var existingProduct = new Product 
        { 
            Id = productId, 
            Name = "Updated Product", 
            Description = "Updated Description", 
            Price = 79.99m, 
            Stock = 10,
            Version = 1
        };
        
        _mockJwtService.Setup(x => x.GenerateToken()).Returns("test-token");
        _mockProductReadRepository.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);
        
        // Mockear delete exitoso
        _mockProductWriteRepository.Setup(x => x.DeleteAsync(existingProduct, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        _mockUnitOfWork.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        _mockUnitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        using var client = CreateClient();

        // Act
        var response = await client.DeleteAsync($"/api/products/{productId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        var productId = 999;
        
        _mockJwtService.Setup(x => x.GenerateToken()).Returns("test-token");
        
        // Mockear delete fallido
        _mockProductWriteRepository.Setup(x => x.DeleteAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        using var client = CreateClient();

        // Act
        var response = await client.DeleteAsync($"/api/products/{productId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task IncreaseStock_WithValidData_ReturnsNoContent()
    {
        // Arrange
        var productId = 1;
        var quantity = 5;
    
        _mockJwtService.Setup(x => x.GenerateToken()).Returns("test-token");
        
        _mockProductWriteRepository.Setup(x => x.IncreaseStockAsync(productId, quantity, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        using var client = CreateClient();

        // Act
        var response = await client.PostAsync($"/api/products/{productId}/increase-stock?qty={quantity}", null);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    
        // Verify that all operations were called
        _mockProductWriteRepository.Verify(x => x.IncreaseStockAsync(productId, quantity, It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IncreaseStock_WhenRepositoryThrowsException_ReturnsBadRequest()
    {
        // Arrange
        var productId = 1;
        var quantity = 5;
        var errorMessage = "Product not found";
    
        _mockJwtService.Setup(x => x.GenerateToken()).Returns("test-token");
    
        // Mockear que el repositorio lanza una excepción
        _mockProductWriteRepository.Setup(x => x.IncreaseStockAsync(productId, quantity, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(errorMessage));
    
        _mockUnitOfWork.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    
        // Not call to commit because it's fail before
        _mockUnitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        using var client = CreateClient();

        // Act
        var response = await client.PostAsync($"/api/products/{productId}/increase-stock?qty={quantity}", null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains(errorMessage, content);
    
        // Verify that don't call commit
        _mockUnitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
public async Task AllEndpoints_AddAuthorizationHeader_ExceptIncreaseStock()
{
    // Arrange
    var expectedToken = "test-jwt-token";
    _mockJwtService.Setup(x => x.GenerateToken()).Returns(expectedToken);
    
    // Mocks' configuration
    _mockProductReadRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(new List<Product>());
    _mockProductReadRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync((Product?)null);
    _mockProductWriteRepository.Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync((Product product, CancellationToken _) => product);
    _mockProductWriteRepository.Setup(x => x.GetTrackedByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new Product { Id = 1, Name = "Test", Price = 10m, Stock = 1 });
    _mockProductWriteRepository.Setup(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync((Product product, CancellationToken _) => product);
    _mockProductWriteRepository.Setup(x => x.DeleteAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);
    _mockProductWriteRepository.Setup(x => x.IncreaseStockAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);
    _mockUnitOfWork.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);
    _mockUnitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

    using var client = CreateClient();

    // Act & Assert para GET /api/products
    var getProductsResponse = await client.GetAsync("/api/products");
    Assert.True(getProductsResponse.Headers.Contains("Authorization"));

    // Act & Assert para GET /api/products/1
    var getProductResponse = await client.GetAsync("/api/products/1");
    Assert.True(getProductResponse.Headers.Contains("Authorization"));

    // Act & Assert para POST /api/products
    var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/products");
    createRequest.Content = new StringContent(JsonSerializer.Serialize(new { Name = "Test", Price = 10m, Stock = 1 }), Encoding.UTF8, "application/json");
    var createResponse = await client.SendAsync(createRequest);
    Assert.True(createResponse.Headers.Contains("Authorization"));

    // Act & Assert para PUT /api/products/1
    var updateRequest = new HttpRequestMessage(HttpMethod.Put, "/api/products/1");
    updateRequest.Content = new StringContent(JsonSerializer.Serialize(new { Name = "Test", Price = 10m }), Encoding.UTF8, "application/json");
    var updateResponse = await client.SendAsync(updateRequest);
    Assert.True(updateResponse.Headers.Contains("Authorization"));

    // Act & Assert para DELETE /api/products/1
    var deleteResponse = await client.DeleteAsync("/api/products/1");
    Assert.True(deleteResponse.Headers.Contains("Authorization"));

    // IncreaseStock NO debe tener Authorization header
    var increaseStockResponse = await client.PostAsync("/api/products/1/increase-stock?qty=5", null);
    Assert.False(increaseStockResponse.Headers.Contains("Authorization"));
}
}