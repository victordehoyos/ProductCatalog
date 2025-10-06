namespace ProductCatalogAPI.Application.DTOs;

public class OrderDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Total { get; set; }
    public DateTime Date { get; set; }

    public OrderDto() { }
    
    public OrderDto(int id, int productId, int quantity, decimal total, DateTime date)
    {
        Id = id;
        ProductId = productId;
        Quantity = quantity;
        Total = total;
        Date = date;
    }

}