using Inventario.Core.Enums;

namespace Inventario.Core.Entities;

public class Cotizacion
{
    public int Id { get; set; }
    public string Folio { get; set; } = string.Empty;
    public string? ClienteNombre { get; set; }
    public string? ClienteContacto { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public DateTime? FechaVigencia { get; set; }
    public EstadoCotizacion Estado { get; set; } = EstadoCotizacion.Pendiente;
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal Impuestos { get; set; }
    public decimal Total { get; set; }

    public int SucursalId { get; set; }
    public Sucursal Sucursal { get; set; } = null!;

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public ICollection<DetalleCotizacion> Detalles { get; set; } = new List<DetalleCotizacion>();
}
