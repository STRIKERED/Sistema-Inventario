using Inventario.Core.Enums;

namespace Inventario.Core.Entities;

public class Venta
{
    public int Id { get; set; }
    public string Folio { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public MetodoPago MetodoPago { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal Impuestos { get; set; }
    public decimal Total { get; set; }

    public int SucursalId { get; set; }
    public Sucursal Sucursal { get; set; } = null!;

    public int CorteDeCajaId { get; set; }
    public CorteDeCaja CorteDeCaja { get; set; } = null!;

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
}
