using ProductCatalogAPI.Domain.Entities;

namespace ProductCatalogAPI.UnitTests.Shared.Builders {
    public class ProductBuilder
    {
        private Product _product;

        public ProductBuilder()
        {
            _product = new Product
            {
                Id = 1,
                Name = "Test Product",
                Description = "Test Description",
                Price = 100.0m,
                Stock = 3,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Version = 0
            };
        }
        
        public static Product BuildDefault()
        {
            return new ProductBuilder().Build();
        }
        
        public static ProductBuilder Create() => new ProductBuilder();
        
        public ProductBuilder WithId(int id)
        {
            _product.Id = id;
            return this;
        }

        public ProductBuilder WithName(string name)
        {
            _product.Name = name;
            return this;
        }

        public ProductBuilder WithDescription(string description)
        {
            _product.Description = description;
            return this;
        }

        public ProductBuilder WithPrice(decimal price)
        {
            _product.Price = price;
            return this;
        }

        public ProductBuilder WithStock(int stock)
        {
            _product.Stock = stock;
            return this;
        }

        public ProductBuilder WithVersion(int version)
        {
            _product.Version = version;
            return this;
        }

        public ProductBuilder WithZeroStock()
        {
            _product.Stock = 0;
            return this;
        }

        public ProductBuilder WithLowStock()
        {
            _product.Stock = 10;
            return this;
        }
        
        public Product Build()
        {
            return _product;
        }
    }
}