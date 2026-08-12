using Inventario.Core.Enums;

namespace Inventario.Core.Entities;

public class MovimientoInventario
{
    public int Id { get; set; }
    public TipoMovimientoInventario TipoMovimiento { get; set; }
    public int Cantidad { get; set; }
    public string? Motivo { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    // Sin SucursalId/InventarioId: el Producto ya implica su Inventario.
    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }

    public int? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
}
