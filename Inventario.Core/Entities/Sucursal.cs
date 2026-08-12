namespace Inventario.Core.Entities;

public class Sucursal
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Direccion { get; set; }

    public ICollection<Inventario> Inventarios { get; set; } = new List<Inventario>();
}
