using Inventario.Core.Dtos;
using Inventario.Core.Enums;

namespace Inventario.Desktop.Services.Sesion;

/// <summary>
/// Mantiene en memoria (y persiste en el dispositivo) la sesión del usuario que inició sesión:
/// el JWT, sus datos básicos y el Inventario con el que está operando. Es singleton: toda la
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
    /// Inventario con el que opera el usuario. Se fija tras el login: automático si solo tiene acceso
    /// a uno (ver LoginResponse.Inventarios), o eligiéndolo explícitamente si tiene varios.
    /// </summary>
    int? InventarioOperativoId { get; }

    bool HaySesionActiva { get; }

    /// <summary>Guarda los datos de un login exitoso y los persiste en el dispositivo. No fija el
    /// Inventario operativo: eso lo decide el llamador según LoginResponse.Inventarios.</summary>
    Task IniciarSesionAsync(LoginResponse respuesta);

    /// <summary>Fija (o cambia) el Inventario operativo.</summary>
    Task FijarInventarioOperativoAsync(int inventarioId);

    /// <summary>Intenta recuperar una sesión guardada de un lanzamiento anterior de la app.</summary>
    Task<bool> RestaurarSesionAsync();

    Task CerrarSesionAsync();
}
