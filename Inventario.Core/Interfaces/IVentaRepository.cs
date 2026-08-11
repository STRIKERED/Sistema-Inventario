using Inventario.Core.Entities;

namespace Inventario.Core.Interfaces;

public interface IVentaRepository
{
    Task<Venta?> ObtenerPorIdAsync(int id);
    Task<Venta> CrearAsync(Venta venta);
    Task<IEnumerable<Venta>> ObtenerPorCorteDeCajaAsync(int corteDeCajaId);
}
