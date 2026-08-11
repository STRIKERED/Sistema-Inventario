using Inventario.Core.Entities;

namespace Inventario.Core.Interfaces;

public interface ICotizacionRepository
{
    Task<Cotizacion?> ObtenerPorIdAsync(int id);
    Task<Cotizacion> CrearAsync(Cotizacion cotizacion);
    Task<IEnumerable<Cotizacion>> ObtenerVigentesAsync(int sucursalId);
}
