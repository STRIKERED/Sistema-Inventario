using Inventario.Core.Entities;

namespace Inventario.Core.Interfaces;

public interface IProductoRepository
{
    Task<Producto?> ObtenerPorIdAsync(int id);

    /// <summary>El código de barras ya no es único global: se busca dentro de un Inventario puntual.</summary>
    Task<Producto?> ObtenerPorCodigoBarrasAsync(string codigoBarras, int inventarioId);

    Task<IEnumerable<Producto>> ObtenerPorInventarioAsync(int inventarioId);
    Task AgregarAsync(Producto producto);
    Task ActualizarAsync(Producto producto);
}
