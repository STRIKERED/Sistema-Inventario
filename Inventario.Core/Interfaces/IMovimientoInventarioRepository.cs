using Inventario.Core.Entities;
using Inventario.Core.Enums;

namespace Inventario.Core.Interfaces;

public interface IMovimientoInventarioRepository
{
    Task<MovimientoInventario?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<MovimientoInventario>> ObtenerPorProductoAsync(int productoId);

    /// <summary>Movimientos de todos los Productos de un Inventario (join por Producto.InventarioId,
    /// ya que el movimiento no guarda InventarioId directamente). Filtros todos opcionales — usado
    /// por el historial de Inventario.Web.</summary>
    Task<IEnumerable<MovimientoInventario>> ObtenerPorInventarioAsync(
        int inventarioId, DateTime? desde = null, DateTime? hasta = null, TipoMovimientoInventario? tipo = null);

    Task<MovimientoInventario> CrearAsync(MovimientoInventario movimiento);
}
