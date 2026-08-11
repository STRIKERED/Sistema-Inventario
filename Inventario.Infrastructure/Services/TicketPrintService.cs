using System.Text;
using Inventario.Core.Entities;
using Inventario.Core.Interfaces;

namespace Inventario.Infrastructure.Services;

/// <summary>Genera e imprime tickets de venta usando comandos ESC/POS para impresoras térmicas.</summary>
public class TicketPrintService : ITicketPrintService
{
    private const byte Esc = 0x1B;
    private const byte Gs = 0x1D;
    private const int AnchoTicket = 32; // caracteres por línea, típico en impresoras térmicas de 58mm

    public byte[] GenerarTicketEscPos(Venta venta)
    {
        using var ms = new MemoryStream();

        // Inicializar impresora
        ms.WriteByte(Esc);
        ms.WriteByte(0x40);

        EscribirCentrado(ms, "TICKET DE VENTA");
        EscribirLinea(ms, $"Folio: {venta.Folio}");
        EscribirLinea(ms, $"Fecha: {venta.Fecha:dd/MM/yyyy HH:mm}");
        EscribirSeparador(ms);

        foreach (var detalle in venta.Detalles)
        {
            var nombre = detalle.Producto?.Nombre ?? $"Producto #{detalle.ProductoId}";
            var importe = detalle.Cantidad * detalle.PrecioUnitario - detalle.DescuentoUnitario * detalle.Cantidad;

            EscribirLinea(ms, nombre);
            EscribirLinea(ms, $"  {detalle.Cantidad} x {detalle.PrecioUnitario:C2} = {importe:C2}");
        }

        EscribirSeparador(ms);
        EscribirLineaDerecha(ms, $"Subtotal: {venta.Subtotal:C2}");
        EscribirLineaDerecha(ms, $"Descuento: {venta.Descuento:C2}");
        EscribirLineaDerecha(ms, $"Impuestos: {venta.Impuestos:C2}");

        // Total en negritas
        ms.WriteByte(Esc);
        ms.WriteByte(0x45);
        ms.WriteByte(0x01);
        EscribirLineaDerecha(ms, $"TOTAL: {venta.Total:C2}");
        ms.WriteByte(Esc);
        ms.WriteByte(0x45);
        ms.WriteByte(0x00);

        EscribirLinea(ms, $"Metodo de pago: {venta.MetodoPago}");
        EscribirSeparador(ms);
        EscribirCentrado(ms, "Gracias por su compra");

        // Avanzar papel y cortar (corte parcial)
        ms.WriteByte(Esc);
        ms.WriteByte(0x64);
        ms.WriteByte(0x04);
        ms.WriteByte(Gs);
        ms.WriteByte(0x56);
        ms.WriteByte(0x01);

        return ms.ToArray();
    }

    public Task ImprimirTicketAsync(Venta venta, string nombreImpresora)
    {
        if (string.IsNullOrWhiteSpace(nombreImpresora))
        {
            throw new ArgumentException("Debe indicar el nombre de la impresora.", nameof(nombreImpresora));
        }

        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException(
                "La impresión directa RAW solo está soportada en Windows. " +
                "Use GenerarTicketEscPos para obtener los bytes y enviarlos por otro medio (red, USB, etc.).");
        }

        var datos = GenerarTicketEscPos(venta);
        return Task.Run(() => RawPrinterHelper.EnviarBytes(nombreImpresora, datos, $"Ticket-{venta.Folio}"));
    }

    private static void EscribirLinea(Stream stream, string texto)
    {
        var bytes = Encoding.ASCII.GetBytes(texto + "\n");
        stream.Write(bytes, 0, bytes.Length);
    }

    private static void EscribirCentrado(Stream stream, string texto)
    {
        var relleno = Math.Max(0, (AnchoTicket - texto.Length) / 2);
        EscribirLinea(stream, new string(' ', relleno) + texto);
    }

    private static void EscribirLineaDerecha(Stream stream, string texto)
    {
        var relleno = Math.Max(0, AnchoTicket - texto.Length);
        EscribirLinea(stream, new string(' ', relleno) + texto);
    }

    private static void EscribirSeparador(Stream stream)
    {
        EscribirLinea(stream, new string('-', AnchoTicket));
    }
}
