namespace ProductCatalogAPI.Application.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ProductDto(int id, string name, string description, decimal price, int stock, DateTime createdAt, DateTime? updatedAt)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
        Stock = stock;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public ProductDto() {}
}