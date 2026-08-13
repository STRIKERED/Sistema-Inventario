using Inventario.Core.Entities;

namespace Inventario.Core.Interfaces;

public interface IVentaRepository
{
    Task<Venta?> ObtenerPorIdAsync(int id);
    Task<Venta> CrearAsync(Venta venta);
    Task<IEnumerable<Venta>> ObtenerPorCorteDeCajaAsync(int corteDeCajaId);

    /// <summary>Ventas de un Inventario cuya Fecha cae en [desde, hasta]. Por default trae solo las
    /// no canceladas (Dashboard, historial); con <paramref name="cancelada"/> en true trae solo las
    /// canceladas (vista "Ventas canceladas" de Inventario.Web).</summary>
    Task<IEnumerable<Venta>> ObtenerPorInventarioAsync(int inventarioId, DateTime desde, DateTime hasta, bool cancelada = false);

    /// <summary>Fija el folio de una venta ya insertada (se genera después del insert, a partir de su Id).</summary>
    Task ActualizarFolioAsync(int id, string folio);
}
