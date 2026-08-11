using Inventario.Core.Dtos;
using Inventario.Core.Enums;

namespace Inventario.Desktop.Services.Sesion;

/// <summary>
/// Mantiene en memoria (y persiste en el dispositivo) la sesión del usuario que inició sesión:
/// el JWT, sus datos básicos y la sucursal con la que está operando la caja. Es singleton: toda la
/// app comparte la misma instancia durante su ciclo de vida.
/// </summary>
public interface ISessionService
{
    string? Token { get; }
    int? UsuarioId { get; }
    string? NombreUsuario { get; }
    string? NombreCompleto { get; }
    RolUsuario? Rol { get; }

    /// <summary>
    /// Sucursal con la que opera el usuario. Para roles con SucursalId fijo viene del propio login;
    /// para Administrador (que puede no tener sucursal fija) se completa con la que elija tras entrar.
    /// </summary>
    int? SucursalOperativaId { get; }

    bool HaySesionActiva { get; }

    /// <summary>Guarda la sesión resultante de un login exitoso y la persiste en el dispositivo.</summary>
    Task IniciarSesionAsync(LoginResponse respuesta);

    /// <summary>Fija (o cambia) la sucursal operativa, para los casos en que el usuario no trae una fija.</summary>
    Task FijarSucursalOperativaAsync(int sucursalId);

    /// <summary>Intenta recuperar una sesión guardada de un lanzamiento anterior de la app.</summary>
    Task<bool> RestaurarSesionAsync();

    Task CerrarSesionAsync();
}
