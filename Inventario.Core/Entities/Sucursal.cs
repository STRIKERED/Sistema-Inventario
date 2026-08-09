namespace Inventario.Core.Entities;

public class Sucursal
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Direccion { get; set; }

    public ICollection<StockPorSucursal> Stocks { get; set; } = new List<StockPorSucursal>();
    public ICollection<Caja> Cajas { get; set; } = new List<Caja>();
}
