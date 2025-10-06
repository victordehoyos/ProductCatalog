using Microsoft.AspNetCore.Mvc;
using ProductCatalogAPI.Application.DTOs;
using ProductCatalogAPI.Application.UseCases.Products;
using ProductCatalogAPI.Infrastructure.Services;

namespace ProductCatalogAPI.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IJwtService _jwtService;

    public ProductsController(IJwtService jwtService)
    {
        _jwtService = jwtService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetProducts([FromServices] GetAllProductsUseCase useCase, 
        CancellationToken ct)
    {
        try
        {
            var token = _jwtService.GenerateToken();
            Response.Headers.Append("Authorization", $"Bearer {token}");

            var products = await useCase.ExecuteAsync(ct);
            return Ok(products);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id, 
        [FromServices] GetProductByIdUseCase useCase, CancellationToken ct)
    {
        try
        {
            var token = _jwtService.GenerateToken();
            Response.Headers.Append("Authorization", $"Bearer {token}");

            var product = await useCase.ExecuteAsync(id, ct);
            if (product == null)
                return NotFound();

            return Ok(product);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto productDto, 
        [FromServices] CreateProductUseCase useCase, CancellationToken ct)
    {
        try
        {
            var token = _jwtService.GenerateToken();
            Response.Headers.Append("Authorization", $"Bearer {token}");

            var createdProduct = await useCase.ExecuteAsync(productDto, ct);
            return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(
        int id, 
        [FromBody] UpdateProductDto dto,
        [FromServices] UpdateProductUseCase useCase,
        CancellationToken ct)
    {
        try
        {
            var token = _jwtService.GenerateToken();
            Response.Headers.Append("Authorization", $"Bearer {token}");

            var updatedProduct = await useCase.ExecuteAsync(id, dto, ct);
            if (updatedProduct == null)
                return NoContent();

            return Ok(updatedProduct);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteProduct(int id, [FromServices] DeleteProductUseCase useCase, CancellationToken ct)
    {
        try
        {
            var token = _jwtService.GenerateToken();
            Response.Headers.Append("Authorization", $"Bearer {token}");

            var result = await useCase.ExecuteAsync(id, ct);
            if (!result)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    
    [HttpPost("{id}/increase-stock")]
    public async Task<IActionResult> IncreaseStock(
        int id, 
        [FromQuery] int qty,
        [FromServices] IncreaseStockUseCase useCase, 
        CancellationToken ct)
    {
        try
        {
            await useCase.ExecuteAsync(id, qty, ct);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}