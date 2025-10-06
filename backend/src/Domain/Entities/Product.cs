using System.ComponentModel.DataAnnotations;

namespace ProductCatalogAPI.Domain.Entities;

public class Product
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [ConcurrencyCheck]
    public int Version { get; set; } = 0;

    public Product(string name, string description, decimal price)
    {
        Name = name;
        Description = description;
        Price = price;
    }
    public Product(string name, string description, decimal price, int stock, DateTime createdAt, int version)
    {
        Name = name;
        Description = description;
        Price = price;
        Stock = stock;
        CreatedAt = createdAt;
        Version = version;
    } 
    
    public Product() {}
}