using Inventario.Core.Interfaces;

namespace Inventario.Infrastructure.Services;

public class FolioService : IFolioService
{
    public string GenerarFolioVenta(int ventaId) => $"V-{ventaId:D6}";

    public string GenerarFolioCotizacion(int cotizacionId) => $"C-{cotizacionId:D6}";
}
