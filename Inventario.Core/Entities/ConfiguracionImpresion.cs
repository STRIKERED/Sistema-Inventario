namespace Inventario.Core.Entities;

/// <summary>
/// Configuración de impresión de tickets y cotizaciones para un Inventario. Cada Inventario puede
/// tener su propia impresora térmica conectada, por lo que esto se guarda en base de datos en vez
/// de hardcodearse.
/// </summary>
public class ConfiguracionImpresion
{
    public int Id { get; set; }

    public int InventarioId { get; set; }
    public Inventario? Inventario { get; set; }

    /// <summary>Nombre de la impresora compartida en Windows, ej. "POS-80".</summary>
    public string NombreImpresora { get; set; } = string.Empty;

    /// <summary>Ancho del rollo térmico en milímetros: 58 u 80.</summary>
    public int AnchoTicketMm { get; set; } = 80;

    /// <summary>Texto que encabeza el ticket, ej. nombre del negocio.</summary>
    public string? EncabezadoTicket { get; set; }

    /// <summary>Texto al pie del ticket, ej. "Gracias por su compra".</summary>
    public string? PiePaginaTicket { get; set; }

    /// <summary>Ruta al logo usado en las cotizaciones en PDF.</summary>
    public string? LogoRutaPdf { get; set; }
}
