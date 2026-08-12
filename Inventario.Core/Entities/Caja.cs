namespace Inventario.Core.Entities;

public class Caja
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;

    public int InventarioId { get; set; }
    public Inventario? Inventario { get; set; }
}
