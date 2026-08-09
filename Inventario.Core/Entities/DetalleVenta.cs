namespace Inventario.Core.Entities;

public class DetalleVenta
{
    public int Id { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal DescuentoUnitario { get; set; }

    public decimal Subtotal => (Cantidad * PrecioUnitario) - DescuentoUnitario;

    public int VentaId { get; set; }
    public Venta Venta { get; set; } = null!;

    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
}
