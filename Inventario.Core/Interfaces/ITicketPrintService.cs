using Inventario.Core.Entities;

namespace Inventario.Core.Interfaces;

/// <summary>Genera e imprime tickets de venta en formato ESC/POS para impresoras térmicas.</summary>
public interface ITicketPrintService
{
    /// <summary>
    /// Genera el flujo de bytes ESC/POS del ticket, sin enviarlo a ninguna impresora.
    /// Si no se indica <paramref name="configuracion"/>, se usan valores por defecto
    /// (58mm, encabezado/pie genéricos) sin consultar la configuración guardada.
    /// </summary>
    byte[] GenerarTicketEscPos(Venta venta, ConfiguracionImpresion? configuracion = null);

    /// <summary>
    /// Genera el ticket y lo envía a la impresora configurada para el Inventario de la venta.
    /// Lanza <see cref="InvalidOperationException"/> si el Inventario no tiene una
    /// <see cref="ConfiguracionImpresion"/> asociada.
    /// </summary>
    Task ImprimirTicketAsync(Venta venta);
}
