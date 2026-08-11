using System.Text.Json.Serialization;
using Inventario.Core.Enums;

namespace Inventario.Core.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;

    // JsonIgnore a nivel de entidad (no solo en UsuariosController): Usuario también viaja anidado
    // dentro de Venta, Cotizacion, CorteDeCaja y MovimientoInventario vía sus navegaciones,
    // y ese hash nunca debe salir por ningún endpoint.
    [JsonIgnore]
    public string PasswordHash { get; set; } = string.Empty;
    public string? NombreCompleto { get; set; }
    public RolUsuario Rol { get; set; }
    public bool Activo { get; set; } = true;

    public int? SucursalId { get; set; }
    public Sucursal? Sucursal { get; set; }
}
