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

    public int InventarioId { get; set; }
    public Inventario? Inventario { get; set; }

    public int CorteDeCajaId { get; set; }
    public CorteDeCaja? CorteDeCaja { get; set; }

    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    // Columna nueva: no existía en el esquema original, se agrega vía migración
    // porque VentaRepository.ObtenerPorCorteDeCajaAsync() filtra ventas no canceladas.
    public bool Cancelada { get; set; }

    public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
}
