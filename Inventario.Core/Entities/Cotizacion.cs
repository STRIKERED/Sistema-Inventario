using Inventario.Core.Enums;

namespace Inventario.Core.Entities;

public class Cotizacion
{
    public int Id { get; set; }
    public string Folio { get; set; } = string.Empty;
    public string? ClienteNombre { get; set; }
    public string? ClienteContacto { get; set; }

    // Mapea a la columna "Fecha" en la base de datos (ver InventarioDbContext);
    // se llama FechaCreacion en el modelo porque así la usa CotizacionRepository.
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaVigencia { get; set; }
    public EstadoCotizacion Estado { get; set; } = EstadoCotizacion.Vigente;
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal Impuestos { get; set; }
    public decimal Total { get; set; }

    public int InventarioId { get; set; }
    public Inventario? Inventario { get; set; }

    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public ICollection<DetalleCotizacion> Detalles { get; set; } = new List<DetalleCotizacion>();
}
