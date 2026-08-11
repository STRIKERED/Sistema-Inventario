using Inventario.Core.Dtos;
using Inventario.Core.Entities;

namespace Inventario.Core.Mapping;

public static class StockPorSucursalMapping
{
    public static StockPorSucursalDto ToDto(this StockPorSucursal stock) =>
        new(stock.Id, stock.ProductoId, stock.Producto?.Nombre, stock.SucursalId, stock.Sucursal?.Nombre, stock.Cantidad);

    public static IEnumerable<StockPorSucursalDto> ToDto(this IEnumerable<StockPorSucursal> stocks) =>
        stocks.Select(s => s.ToDto());
}
