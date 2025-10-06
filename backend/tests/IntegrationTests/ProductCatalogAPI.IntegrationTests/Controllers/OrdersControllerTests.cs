using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ProductCatalogAPI.Application.DTOs;
using ProductCatalogAPI.Application.Exceptions;
using ProductCatalogAPI.Application.UseCases.Orders;
using ProductCatalogAPI.Domain.Interfaces.Read;
using ProductCatalogAPI.Domain.Interfaces.Write;
using ProductCatalogAPI.Infrastructure.Services;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.TestHost;
using ProductCatalogAPI.Application.Ports;
using ProductCatalogAPI.Domain.Entities;

namespace ProductCatalogAPI.IntegrationTests.Controllers;

public class OrdersControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly Mock<IOrderReadRepository> _mockOrderReadRepository;
    private readonly Mock<IOrderWriteRepository> _mockOrderWriteRepository;
    private readonly Mock<IProductReadRepository> _mockProductReadRepository;
    private readonly Mock<IProductWriteRepository> _mockProductWriteRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;

    public OrdersControllerTests()
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
                    // Mockear solo las dependencias de infraestructura
                    services.AddScoped<IJwtService>(_ => _mockJwtService.Object);
                    services.AddScoped<IOrderReadRepository>(_ => _mockOrderReadRepository.Object);
                    services.AddScoped<IOrderWriteRepository>(_ => _mockOrderWriteRepository.Object);
                    services.AddScoped<IProductReadRepository>(_ => _mockProductReadRepository.Object);
                    services.AddScoped<IProductWriteRepository>(_ => _mockProductWriteRepository.Object);
                    services.AddScoped<IUnitOfWork>(_ => _mockUnitOfWork.Object);
                    
                    // Los use cases se crearán automáticamente con los mocks inyectados
                });
            });
    }

    private HttpClient CreateClient()
    {
        return _factory.CreateClient();
    }

    [Fact]
    public async Task GetOrders_ReturnsOkWithOrdersAndAuthorizationHeader()
    {
        // Arrange
        var expectedToken = "test-jwt-token";
        var expectedOrders = new List<Order>
        {
            new Order { Id = 1, ProductId = 1, Quantity = 2, Total = 50.00m, Date = DateTime.UtcNow },
            new Order { Id = 2, ProductId = 2, Quantity = 1, Total = 25.50m, Date = DateTime.UtcNow.AddHours(-1) }
        };

        _mockJwtService.Setup(x => x.GenerateToken()).Returns(expectedToken);
        _mockOrderReadRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOrders);

        using var client = CreateClient();

        // Act
        var response = await client.GetAsync("/api/orders");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        // Verificar header de autorización
        Assert.True(response.Headers.Contains("Authorization"));
        var authHeader = response.Headers.GetValues("Authorization").First();
        Assert.Equal($"Bearer {expectedToken}", authHeader);

        // Verificar contenido
        var content = await response.Content.ReadAsStringAsync();
        var orders = JsonSerializer.Deserialize<List<OrderDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(orders);
        Assert.Equal(2, orders.Count);
    }

    [Fact]
    public async Task GetOrders_ReturnsEmptyListWhenNoOrders()
    {
        // Arrange
        _mockJwtService.Setup(x => x.GenerateToken()).Returns("test-token");
        _mockOrderReadRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Order>());

        using var client = CreateClient();

        // Act
        var response = await client.GetAsync("/api/orders");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var orders = JsonSerializer.Deserialize<List<OrderDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(orders);
        Assert.Empty(orders);
    }

    [Fact]
    public async Task GetOrder_WithValidId_ReturnsOrder()
    {
        // Arrange
        var orderId = 1;
        var expectedOrder = new Order { Id = orderId, ProductId = 1, Quantity = 2, Total = 50.00m, Date = DateTime.UtcNow };
        
        _mockJwtService.Setup(x => x.GenerateToken()).Returns("test-token");
        _mockOrderReadRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOrder);

        using var client = CreateClient();

        // Act
        var response = await client.GetAsync($"/api/orders/{orderId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var order = JsonSerializer.Deserialize<OrderDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(order);
        Assert.Equal(expectedOrder.Id, order.Id);
        Assert.Equal(expectedOrder.ProductId, order.ProductId);
        Assert.Equal(expectedOrder.Total, order.Total);
    }

    [Fact]
    public async Task GetOrder_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        var orderId = 999;
        
        _mockJwtService.Setup(x => x.GenerateToken()).Returns("test-token");
        _mockOrderReadRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        using var client = CreateClient();

        // Act
        var response = await client.GetAsync($"/api/orders/{orderId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_WithValidData_ReturnsCreated()
    {
    // Arrange
    var createOrderDto = new CreateOrderDto { ProductId = 1, Quantity = 2 };
    var product = new Product { Id = 1, Name = "Test Product", Price = 25.00m, Stock = 10 };
    var expectedOrder = new Order { Id = 1, ProductId = 1, Quantity = 2, Total = 50.00m, Date = DateTime.UtcNow };
    var idempotencyKey = "test-key-123";
    
    _mockJwtService.Setup(x => x.GenerateToken()).Returns("test-token");
    _mockOrderReadRepository.Setup(x => x.GetByIdempotencyKeyAsync(idempotencyKey, It.IsAny<CancellationToken>()))
        .ReturnsAsync((Order?)null);
    _mockProductWriteRepository.Setup(x => x.TryDecreaseStockAdminAsync(1, 2, It.IsAny<CancellationToken>()))
        .ReturnsAsync(true);
    _mockProductReadRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
        .ReturnsAsync(product);
    Order capturedOrder = null;
    _mockOrderWriteRepository.Setup(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
        .Callback<Order, CancellationToken>((order, _) =>
        {
            capturedOrder = order;
            order.Id = 1;
        }).ReturnsAsync((Order order, CancellationToken _) => order);
    
    _mockUnitOfWork.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);
    _mockUnitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

    using var client = CreateClient();

    var jsonContent = JsonSerializer.Serialize(createOrderDto);
    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
    
    var request = new HttpRequestMessage(HttpMethod.Post, "/api/orders");
    request.Content = content;
    request.Headers.Add("Idempotency-Key", idempotencyKey);

    // Act
    var response = await client.SendAsync(request);
    //response.Content = 

    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    
    // Verificar header Location - usar verificaciones más flexibles
    Assert.NotNull(response.Headers.Location);
    
    // Opción 1: Verificar que contiene el ID esperado
    var location = response.Headers.Location.ToString();
    //Assert.Contains("1", location); // Verificar que contiene el ID
    
    // Opción 2: Verificar que es una URL válida de orders
    Assert.Contains("orders", location.ToLower());
    
    // Opción 3: Verificar el path absoluto
    Assert.Equal("/api/Orders/1".ToLower(), response.Headers.Location.AbsolutePath.ToLower());
    Assert.Equal(1, capturedOrder.Id);
    }

    [Fact]
    public async Task AllEndpoints_AddAuthorizationHeader()
    {
        // Arrange
        var expectedToken = "test-jwt-token";
        _mockJwtService.Setup(x => x.GenerateToken()).Returns(expectedToken);
        var createOrderDto = new CreateOrderDto { ProductId = 1, Quantity = 2 };
        
        // Configurar repositorios para evitar errores 500
        _mockOrderReadRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Order>());
        _mockOrderReadRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);
        _mockOrderWriteRepository.Setup(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Order());
        _mockUnitOfWork.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        using var client = CreateClient();

        // Act & Assert para GET /api/orders
        var getOrdersResponse = await client.GetAsync("/api/orders");
        Assert.True(getOrdersResponse.Headers.Contains("Authorization"));

        // Act & Assert para GET /api/orders/1
        var getOrderResponse = await client.GetAsync("/api/orders/1");
        Assert.True(getOrderResponse.Headers.Contains("Authorization"));

        // Act & Assert para POST /api/orders
        var createOrderRequest = new HttpRequestMessage(HttpMethod.Post, "/api/orders");
        createOrderRequest.Headers.Add("Idempotency-Key", "test-key");
        createOrderRequest.Headers.Add("Authorization", $"Bearer {expectedToken}");
        var jsonContent = JsonSerializer.Serialize(createOrderDto);
        createOrderRequest.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        
        var createOrderResponse = await client.SendAsync(createOrderRequest);
        Assert.True(createOrderResponse.Headers.Contains("Authorization"));
    }
}