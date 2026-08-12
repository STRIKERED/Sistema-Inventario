using Inventario.Core.Entities;

namespace Inventario.Core.Interfaces;

public interface IMovimientoInventarioRepository
{
    Task<MovimientoInventario?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<MovimientoInventario>> ObtenerPorProductoAsync(int productoId);
    Task<MovimientoInventario> CrearAsync(MovimientoInventario movimiento);
}
