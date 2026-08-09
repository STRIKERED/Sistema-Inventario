namespace Inventario.Core.Entities;

public class Caja
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;

    public int SucursalId { get; set; }
    public Sucursal Sucursal { get; set; } = null!;

    public ICollection<CorteDeCaja> Cortes { get; set; } = new List<CorteDeCaja>();
}
