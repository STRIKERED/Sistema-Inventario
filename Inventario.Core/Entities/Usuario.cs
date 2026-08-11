using Inventario.Core.Enums;

namespace Inventario.Core.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? NombreCompleto { get; set; }
    public RolUsuario Rol { get; set; }
    public bool Activo { get; set; } = true;

    public int? SucursalId { get; set; }
    public Sucursal? Sucursal { get; set; }
}
