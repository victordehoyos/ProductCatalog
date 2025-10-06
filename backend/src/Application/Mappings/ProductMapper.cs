using ProductCatalogAPI.Application.DTOs;
using ProductCatalogAPI.Domain.Entities;

namespace ProductCatalogAPI.Application.Mappings;

public static class ProductMapper
{
    public static ProductDto ToDto(Product product)
    {
        return new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Stock,
            product.CreatedAt,
            product.UpdatedAt);
    }
    
    public static Product ToEntity(ProductDto product)
    {
        var p = new Product();
        p.Id = product.Id;
        p.Name = product.Name;
        p.Description = product.Description;
        p.Price = product.Price;
        p.Stock = product.Stock;
        p.CreatedAt = product.CreatedAt;
        p.UpdatedAt = product.UpdatedAt;

        return p;
    } 

    public static Product ToEntity(UpdateProductDto product)
    {
        return new Product(
            product.Name,
            product.Description,
            product.Price);
    }

    public static List<ProductDto> ToDtoList(IEnumerable<Product> products)
    {
        return products.Select(ToDto).ToList();
    }
}