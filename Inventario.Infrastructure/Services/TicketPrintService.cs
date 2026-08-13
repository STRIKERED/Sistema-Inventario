using System.Text;
using Inventario.Core.Entities;
using Inventario.Core.Interfaces;

namespace Inventario.Infrastructure.Services;

/// <summary>Genera e imprime tickets de venta usando comandos ESC/POS para impresoras térmicas.</summary>
public class TicketPrintService : ITicketPrintService
{
    private const byte Esc = 0x1B;
    private const byte Gs = 0x1D;
    private const int AnchoTicketPorDefecto = 32; // caracteres por línea, típico en impresoras térmicas de 58mm
    private const string EncabezadoPorDefecto = "TICKET DE VENTA";
    private const string PiePaginaPorDefecto = "Gracias por su compra";

    private readonly IConfiguracionImpresionRepository _configuracionRepository;

    public TicketPrintService(IConfiguracionImpresionRepository configuracionRepository)
    {
        _configuracionRepository = configuracionRepository;
    }

    public byte[] GenerarTicketEscPos(Venta venta, ConfiguracionImpresion? configuracion = null)
    {
        var anchoTicket = AnchoEnCaracteres(configuracion?.AnchoTicketMm);
        var encabezado = string.IsNullOrWhiteSpace(configuracion?.EncabezadoTicket)
            ? EncabezadoPorDefecto
            : configuracion!.EncabezadoTicket!;
        var piePagina = string.IsNullOrWhiteSpace(configuracion?.PiePaginaTicket)
            ? PiePaginaPorDefecto
            : configuracion!.PiePaginaTicket!;

        using var ms = new MemoryStream();

        // Inicializar impresora
        ms.WriteByte(Esc);
        ms.WriteByte(0x40);

        EscribirCentrado(ms, encabezado, anchoTicket);
        EscribirLinea(ms, $"Folio: {venta.Folio}");
        EscribirLinea(ms, $"Fecha: {venta.Fecha:dd/MM/yyyy HH:mm}");
        EscribirSeparador(ms, anchoTicket);

        foreach (var detalle in venta.Detalles)
        {
            var nombre = detalle.Producto?.Nombre ?? $"Producto #{detalle.ProductoId}";
            var importe = detalle.Cantidad * detalle.PrecioUnitario - detalle.DescuentoUnitario * detalle.Cantidad;

            EscribirLinea(ms, nombre);
            EscribirLinea(ms, $"  {detalle.Cantidad} x {detalle.PrecioUnitario:C2} = {importe:C2}");
        }

        EscribirSeparador(ms, anchoTicket);
        EscribirLineaDerecha(ms, $"Subtotal: {venta.Subtotal:C2}", anchoTicket);
        EscribirLineaDerecha(ms, $"Descuento: {venta.Descuento:C2}", anchoTicket);
        EscribirLineaDerecha(ms, $"Impuestos: {venta.Impuestos:C2}", anchoTicket);

        // Total en negritas
        ms.WriteByte(Esc);
        ms.WriteByte(0x45);
        ms.WriteByte(0x01);
        EscribirLineaDerecha(ms, $"TOTAL: {venta.Total:C2}", anchoTicket);
        ms.WriteByte(Esc);
        ms.WriteByte(0x45);
        ms.WriteByte(0x00);

        EscribirLinea(ms, $"Metodo de pago: {venta.MetodoPago}");
        EscribirSeparador(ms, anchoTicket);
        EscribirCentrado(ms, piePagina, anchoTicket);

        // Avanzar papel y cortar (corte parcial)
        ms.WriteByte(Esc);
        ms.WriteByte(0x64);
        ms.WriteByte(0x04);
        ms.WriteByte(Gs);
        ms.WriteByte(0x56);
        ms.WriteByte(0x01);

        return ms.ToArray();
    }

    public async Task ImprimirTicketAsync(Venta venta)
    {
        var configuracion = await _configuracionRepository.ObtenerPorInventarioAsync(venta.InventarioId)
            ?? throw new InvalidOperationException(
                $"No hay configuración de impresión para el Inventario #{venta.InventarioId}.");

        if (string.IsNullOrWhiteSpace(configuracion.NombreImpresora))
        {
            throw new InvalidOperationException(
                $"La configuración de impresión del Inventario #{venta.InventarioId} no tiene una impresora asignada.");
        }

        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException(
                "La impresión directa RAW solo está soportada en Windows. " +
                "Use GenerarTicketEscPos para obtener los bytes y enviarlos por otro medio (red, USB, etc.).");
        }

        var datos = GenerarTicketEscPos(venta, configuracion);
        await Task.Run(() => RawPrinterHelper.EnviarBytes(configuracion.NombreImpresora, datos, $"Ticket-{venta.Folio}"));
    }

    private static int AnchoEnCaracteres(int? anchoTicketMm)
    {
        return anchoTicketMm switch
        {
            null => AnchoTicketPorDefecto,
            58 => 32,
            _ => 42, // 80mm (y cualquier otro ancho no contemplado explícitamente)
        };
    }

    private static void EscribirLinea(Stream stream, string texto)
    {
        var bytes = Encoding.ASCII.GetBytes(texto + "\n");
        stream.Write(bytes, 0, bytes.Length);
    }

    private static void EscribirCentrado(Stream stream, string texto, int anchoTicket)
    {
        var relleno = Math.Max(0, (anchoTicket - texto.Length) / 2);
        EscribirLinea(stream, new string(' ', relleno) + texto);
    }

    private static void EscribirLineaDerecha(Stream stream, string texto, int anchoTicket)
    {
        var relleno = Math.Max(0, anchoTicket - texto.Length);
        EscribirLinea(stream, new string(' ', relleno) + texto);
    }

    private static void EscribirSeparador(Stream stream, int anchoTicket)
    {
        EscribirLinea(stream, new string('-', anchoTicket));
    }
}
