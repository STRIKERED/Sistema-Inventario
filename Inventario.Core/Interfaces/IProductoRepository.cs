using Inventario.Core.Entities;

namespace Inventario.Core.Interfaces;

public interface IProductoRepository
{
    Task<Producto?> ObtenerPorIdAsync(int id);
    Task<Producto?> ObtenerPorCodigoBarrasAsync(string codigoBarras);
    Task<IEnumerable<Producto>> ObtenerTodosAsync();
    Task AgregarAsync(Producto producto);
    Task ActualizarAsync(Producto producto);
}
