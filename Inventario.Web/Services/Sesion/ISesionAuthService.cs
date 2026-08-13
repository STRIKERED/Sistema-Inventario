using Inventario.Core.Dtos;

namespace Inventario.Web.Services.Sesion;

/// <summary>Escribe la sesión (cookie de autenticación): login, cambio de Inventario operativo y logout.
/// Contraparte de escritura de <see cref="ICurrentSessionAccessor"/> (que solo lee).</summary>
public interface ISesionAuthService
{
    /// <summary>Firma la cookie tras un login exitoso. Si el usuario tiene un solo Inventario
    /// accesible lo fija de una vez; si tiene varios, queda pendiente de elegir
    /// (ver ICurrentSessionAccessor.DebeSeleccionarInventario).</summary>
    Task IniciarSesionAsync(LoginResponse respuesta);

    /// <summary>Cambia el Inventario operativo de la sesión actual. Devuelve false sin hacer nada si
    /// <paramref name="inventarioId"/> no está entre los Inventarios accesibles del usuario.</summary>
    Task<bool> FijarInventarioOperativoAsync(int inventarioId);

    Task CerrarSesionAsync();
}
