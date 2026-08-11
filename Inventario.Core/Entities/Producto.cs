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

    // Columna nueva: no existía en el esquema original, se agrega vía migración
    // porque ProductoRepository.ObtenerTodosAsync() filtra por productos activos.
    public bool Activo { get; set; } = true;

    public ICollection<StockPorSucursal> Stocks { get; set; } = new List<StockPorSucursal>();
}
