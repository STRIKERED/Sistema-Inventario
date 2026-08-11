using Inventario.Core.Entities;

namespace Inventario.Core.Interfaces;

/// <summary>Genera e imprime tickets de venta en formato ESC/POS para impresoras térmicas.</summary>
public interface ITicketPrintService
{
    /// <summary>Genera el flujo de bytes ESC/POS del ticket, sin enviarlo a ninguna impresora.</summary>
    byte[] GenerarTicketEscPos(Venta venta);

    /// <summary>Genera el ticket y lo envía directamente a una impresora (por nombre) instalada en el sistema.</summary>
    Task ImprimirTicketAsync(Venta venta, string nombreImpresora);
}
