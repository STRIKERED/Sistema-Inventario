namespace Inventario.Core.Entities;

public class DetalleCotizacion
{
    public int Id { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }

    public int CotizacionId { get; set; }
    public Cotizacion? Cotizacion { get; set; }

    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }
}
