using Inventario.Core.Entities;

namespace Inventario.Core.Interfaces;

public interface IVentaRepository
{
    Task<Venta?> ObtenerPorIdAsync(int id);
    Task<Venta> CrearAsync(Venta venta);
    Task<IEnumerable<Venta>> ObtenerPorCorteDeCajaAsync(int corteDeCajaId);

    /// <summary>Fija el folio de una venta ya insertada (se genera después del insert, a partir de su Id).</summary>
    Task ActualizarFolioAsync(int id, string folio);
}
