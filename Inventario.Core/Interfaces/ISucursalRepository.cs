using Inventario.Core.Entities;

namespace Inventario.Core.Interfaces;

public interface ISucursalRepository
{
    Task<Sucursal?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<Sucursal>> ObtenerTodasAsync();
    Task AgregarAsync(Sucursal sucursal);
    Task ActualizarAsync(Sucursal sucursal);
}
