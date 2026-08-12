namespace Inventario.Core.Entities;

public class Producto
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string CodigoBarras { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Categoria { get; set; }
    public string? Unidad { get; set; }
    public decimal PrecioCosto { get; set; }
    public decimal PrecioVenta { get; set; }
    public bool Activo { get; set; } = true;

    // Stock embebido directo en el producto: cada Producto pertenece a exactamente un Inventario
    // (ya no hay una tabla de stock por sucursal compartida entre varios).
    public int InventarioId { get; set; }
    public Inventario? Inventario { get; set; }
    public int CantidadDisponible { get; set; }
    public int StockMinimo { get; set; }
}
