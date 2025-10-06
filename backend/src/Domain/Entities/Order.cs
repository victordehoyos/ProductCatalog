using System.ComponentModel.DataAnnotations;

namespace ProductCatalogAPI.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    
    [Required]
    public int ProductId { get; set; }
    
    public Product Product { get; set; } = null!;
    
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
    
    [Range(0.01, double.MaxValue)]
    public decimal Total { get; set; }
    
    public DateTime Date { get; set; } = DateTime.UtcNow;
    
    [MaxLength(64)]
    public string? IdempotencyKey { get; set; }
}