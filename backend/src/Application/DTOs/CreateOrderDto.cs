using System.ComponentModel.DataAnnotations;

namespace ProductCatalogAPI.Application.DTOs;

public class CreateOrderDto
{
    [Required]
    public int ProductId { get; set; }
    
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}