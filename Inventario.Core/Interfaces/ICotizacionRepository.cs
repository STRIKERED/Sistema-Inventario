using Inventario.Core.Entities;

namespace Inventario.Core.Interfaces;

public interface ICotizacionRepository
{
    Task<Cotizacion?> ObtenerPorIdAsync(int id);
    Task<Cotizacion> CrearAsync(Cotizacion cotizacion);
    Task<IEnumerable<Cotizacion>> ObtenerVigentesAsync(int inventarioId);
    Task ActualizarAsync(Cotizacion cotizacion);

    /// <summary>Fija el folio de una cotización ya insertada (se genera después del insert, a partir de su Id).</summary>
    Task ActualizarFolioAsync(int id, string folio);
}
