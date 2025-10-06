using ProductCatalogAPI.Application.DTOs;
using ProductCatalogAPI.Application.Exceptions;
using ProductCatalogAPI.Application.Ports;
using ProductCatalogAPI.Domain.Entities;
using ProductCatalogAPI.Domain.Interfaces.Read;
using ProductCatalogAPI.Domain.Interfaces.Write;
using Serilog;

namespace ProductCatalogAPI.Application.UseCases.Orders;

public class CreateOrderUseCase
{
    private readonly IUnitOfWork _uow;
    private readonly IProductReadRepository _productRead;
    private readonly IProductWriteRepository _productWrite;
    private readonly IOrderReadRepository _orderRead;
    private readonly IOrderWriteRepository _orderWrite;
    
    public CreateOrderUseCase (
        IUnitOfWork uow,
        IProductReadRepository productRead,
        IProductWriteRepository productWrite,
        IOrderReadRepository orderRead,
        IOrderWriteRepository orderWrite)
    {
        _uow = uow; 
        _productRead = productRead; 
        _productWrite = productWrite;
        _orderRead = orderRead; 
        _orderWrite = orderWrite;
    }
    
    public async Task<OrderDto> ExecuteAsync(CreateOrderDto dto, string idempotencyKey, CancellationToken ct = default)
    {
        Log.ForContext("IdempotencyKey", idempotencyKey)
            .ForContext("ProductId", dto?.ProductId)
            .ForContext("Quantity", dto?.Quantity)
            .Information("CreateOrder started");
        
        var existing = await _orderRead.GetByIdempotencyKeyAsync(idempotencyKey, ct);
        if (existing is not null)
        {
            Log.Information("Idempotencia encontrada para la llave {IdempotencyKey}, retornando order {OrderId}", idempotencyKey, existing.Id);
            return new OrderDto(existing.Id, existing.ProductId, existing.Quantity, existing.Total, existing.Date);
        }

        await _uow.BeginTransactionAsync(ct);

        var product = await _productRead.GetByIdAsync(dto.ProductId, ct)
                      ?? throw new NotFoundException("Producto no existe");
        
        var ok = await _productWrite.TryDecreaseStockAdminAsync(dto.ProductId, dto.Quantity, ct);
        if (!ok)
        {
            await _uow.RollbackAsync(ct);
            Log.Warning("Stock insuficiente para el ProductId {ProductId}, Qty {Qty}", dto.ProductId, dto.Quantity);
            throw new InsufficientStockException("Stock insuficiente o ya fue tomado por otro pedido.");
        }

        var order = new Order
        {
            ProductId = product.Id,
            Quantity = dto.Quantity,
            Total = product.Price * dto.Quantity,
            Date = DateTime.UtcNow,
            IdempotencyKey = idempotencyKey
        };

        await _orderWrite.AddAsync(order, ct);
        await _uow.CommitAsync(ct);
        
        Log.Information("Orden Creada {OrderId} para producto ProductId {ProductId} Qty {Qty} Total {Total}",
            order.Id, order.ProductId, order.Quantity, order.Total);

        return new OrderDto(order.Id, order.ProductId, order.Quantity, order.Total, order.Date);
    }
}