using Inventario.Core.Dtos;
using Inventario.Core.Enums;

namespace Inventario.Web.Services.Sesion;

/// <summary>
/// Lee la sesión del usuario actual (JWT, datos básicos, Inventario operativo) desde los claims de
/// la cookie de autenticación de la request en curso. Equivalente web de
/// Inventario.Desktop.Services.Sesion.ISessionService, pero de solo lectura: los claims se escriben
/// desde <see cref="ISesionAuthService"/>, no desde aquí.
/// </summary>
public interface ICurrentSessionAccessor
{
    bool HaySesionActiva { get; }
    string? Token { get; }
    int? UsuarioId { get; }
    string? NombreUsuario { get; }
    string? NombreCompleto { get; }
    RolUsuario? Rol { get; }

    /// <summary>Inventario con el que opera la sesión ahora mismo, o null si aún no lo ha elegido
    /// (usuario con acceso a más de uno que todavía no pasó por SeleccionarInventario).</summary>
    int? InventarioOperativoId { get; }
    string? InventarioOperativoNombre { get; }

    /// <summary>Todos los Inventarios a los que este usuario tiene acceso (calculado en el login).</summary>
    IReadOnlyList<InventarioDto> InventariosDisponibles { get; }

    /// <summary>True si ya hay sesión pero todavía falta elegir con qué Inventario operar.</summary>
    bool DebeSeleccionarInventario { get; }
}
