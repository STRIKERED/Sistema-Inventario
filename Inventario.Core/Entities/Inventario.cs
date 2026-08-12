namespace Inventario.Core.Entities;

/// <summary>
/// Un inventario independiente dentro de una Sucursal (p. ej. "Papelería" y "Abarrotes" en la misma
/// tienda). Cada Producto pertenece a exactamente un Inventario, y un Usuario puede tener acceso a
/// varios vía <see cref="UsuarioInventario"/>.
/// </summary>
public class Inventario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;

    public int SucursalId { get; set; }
    public Sucursal? Sucursal { get; set; }

    public ICollection<UsuarioInventario> UsuarioInventarios { get; set; } = new List<UsuarioInventario>();
}
