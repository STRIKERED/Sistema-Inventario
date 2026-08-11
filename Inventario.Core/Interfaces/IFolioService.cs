namespace Inventario.Core.Interfaces;

/// <summary>
/// Genera folios legibles (V-000123, C-000045) a partir del Id autonumérico ya asignado por la base de datos.
/// Se generan DESPUÉS del insert (no antes) para no depender de una secuencia/contador aparte:
/// el Id de identity de SQL Server ya garantiza unicidad sin condiciones de carrera.
/// </summary>
public interface IFolioService
{
    string GenerarFolioVenta(int ventaId);
    string GenerarFolioCotizacion(int cotizacionId);
}
