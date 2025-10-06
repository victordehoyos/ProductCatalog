using ProductCatalogAPI.Application.DTOs;
using ProductCatalogAPI.Application.Ports;
using ProductCatalogAPI.Domain.Interfaces.Read;
using ProductCatalogAPI.Domain.Interfaces.Write;
using Serilog;

namespace ProductCatalogAPI.Application.UseCases.Orders;

public class GetAllOrdersUserCase
{
    private readonly IOrderReadRepository _orderRead;
    
    public GetAllOrdersUserCase (IOrderReadRepository orderRead) => _orderRead = orderRead;

    public async Task<List<OrderDto>> ExecuteAsync(CancellationToken ct = default)
    {
        Log.ForContext("Método: GetAllOrdersUserCase", nameof(ExecuteAsync))
            .Information("Consultando información de ordenes");
        
        var list = await _orderRead.GetAllAsync(ct);
        
        return list.Select(o => new OrderDto(o.Id, o.ProductId, o.Quantity, o.Total, o.Date)).ToList();
    }
    
}