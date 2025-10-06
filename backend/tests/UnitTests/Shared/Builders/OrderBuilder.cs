using ProductCatalogAPI.Domain.Entities;

namespace ProductCatalogAPI.UnitTests.Shared.Builders {
    public class OrderBuilder
    {
        private Order _order;

        public OrderBuilder()
        {
            _order = new Order
            {
                Id = 1,
                ProductId = 1,
                Quantity = 1,
                Total = 100.0m,
                Date = DateTime.UtcNow,
                IdempotencyKey = Guid.NewGuid().ToString()
            };
        }
        
        public static Product BuildDefault()
        {
            return new ProductBuilder().Build();
        }

        public static OrderBuilder Create() => new OrderBuilder();

        public OrderBuilder WithId(int id)
        {
            _order.Id = id;
            return this;
        }

        public OrderBuilder WithProductId(int productId)
        {
            _order.ProductId = productId;
            return this;
        }

        public OrderBuilder WithQuantity(int quantity)
        {
            _order.Quantity = quantity;
            return this;
        }

        public OrderBuilder WithTotal(decimal total)
        {
            _order.Total = total;
            return this;
        }

        public OrderBuilder WithIdempotencyKey(string key)
        {
            _order.IdempotencyKey = key;
            return this;
        }

        public Order Build()
        {
            return _order;
        }
    }
}