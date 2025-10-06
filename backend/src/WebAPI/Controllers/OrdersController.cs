using Microsoft.AspNetCore.Mvc;
using ProductCatalogAPI.Application.DTOs;
using ProductCatalogAPI.Application.UseCases.Orders;
using ProductCatalogAPI.Infrastructure.Services;

namespace ProductCatalogAPI.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IJwtService _jwtService;

    public OrdersController(IJwtService jwtService)
    {
        _jwtService = jwtService;
    }

    [HttpGet]
    public async Task<ActionResult<List<OrderDto>>> GetOrders([FromServices] GetAllOrdersUserCase useCase, 
        CancellationToken ct)
    {
        var token = _jwtService.GenerateToken();
        Response.Headers.Append("Authorization", $"Bearer {token}");
        
        var orders = await useCase.ExecuteAsync(ct);
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(int id, [FromServices] GetOrderByIdUseCase useCase, CancellationToken ct)
    {
        var token = _jwtService.GenerateToken();
        Response.Headers.Append("Authorization", $"Bearer {token}");
        
        var order = await useCase.ExecuteAsync(id, ct);
        if (order == null)
            return NotFound();
        
        return Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder(
        [FromBody] CreateOrderDto dto,
        [FromHeader(Name = "Idempotency-Key")] string key,
        [FromServices] CreateOrderUseCase useCase,
        CancellationToken ct
        )
    {
        var token = _jwtService.GenerateToken();
        Response.Headers.Append("Authorization", $"Bearer {token}");
        
        try
        {
            if (string.IsNullOrWhiteSpace(key)) 
                return BadRequest("Idempotency-Key requerido");
            
            var result = await useCase.ExecuteAsync(dto, key, ct);
            return CreatedAtAction(nameof(GetOrder), routeValues: new { id = result.Id }, value: result); 
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}