namespace Inventario.Core.Entities;

public class StockPorSucursal
{
    public int Id { get; set; }
    public int Cantidad { get; set; }

    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;

    public int SucursalId { get; set; }
    public Sucursal Sucursal { get; set; } = null!;
}
