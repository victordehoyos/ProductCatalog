using ProductCatalogAPI.Application.DTOs;
using ProductCatalogAPI.Domain.Interfaces.Read;
using Serilog;

namespace ProductCatalogAPI.Application.UseCases.Orders;

public class GetOrderByIdUseCase
{
    private readonly IOrderReadRepository _orderRead;
    
    public GetOrderByIdUseCase (IOrderReadRepository orderRead) => _orderRead = orderRead;
    
    public async Task<OrderDto?> ExecuteAsync(int id, CancellationToken ct = default)
    {
        Log.ForContext("ProductId", id)
            .ForContext("Método: GetOrderByIdUseCase ", nameof(ExecuteAsync))
            .Information("Consultando Orden por Id");
        
        var order = await _orderRead.GetByIdAsync(id, ct);
        if (order != null)
        {
            Log.Information("Orden encontrada para Id {id}", id); 
            return new OrderDto(order.Id, order.ProductId, order.Quantity, order.Total, order.Date);
        }
        
        Log.Information("No se encontró una Orden asociada al Id {id}", id); 
        return null;
    }
}