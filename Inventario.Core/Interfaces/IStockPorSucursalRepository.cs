using Inventario.Core.Entities;

namespace Inventario.Core.Interfaces;

public interface IStockPorSucursalRepository
{
    Task<StockPorSucursal?> ObtenerAsync(int productoId, int sucursalId);
    Task<IEnumerable<StockPorSucursal>> ObtenerPorSucursalAsync(int sucursalId);
    Task<IEnumerable<StockPorSucursal>> ObtenerPorProductoAsync(int productoId);
    Task AgregarAsync(StockPorSucursal stock);
    Task ActualizarAsync(StockPorSucursal stock);
}
