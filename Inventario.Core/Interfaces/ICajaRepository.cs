using Inventario.Core.Entities;

namespace Inventario.Core.Interfaces;

public interface ICajaRepository
{
    Task<Caja?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<Caja>> ObtenerPorInventarioAsync(int inventarioId);
    Task AgregarAsync(Caja caja);
    Task ActualizarAsync(Caja caja);
}
