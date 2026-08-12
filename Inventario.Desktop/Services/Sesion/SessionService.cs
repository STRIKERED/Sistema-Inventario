using Inventario.Core.Dtos;
using Inventario.Core.Enums;
using Microsoft.Maui.Storage;

namespace Inventario.Desktop.Services.Sesion;

public class SessionService : ISessionService
{
    private const string ClaveToken = "sesion.token";
    private const string ClaveUsuarioId = "sesion.usuarioId";
    private const string ClaveNombreUsuario = "sesion.nombreUsuario";
    private const string ClaveNombreCompleto = "sesion.nombreCompleto";
    private const string ClaveRol = "sesion.rol";
    private const string ClaveInventarioId = "sesion.inventarioId";

    public string? Token { get; private set; }
    public int? UsuarioId { get; private set; }
    public string? NombreUsuario { get; private set; }
    public string? NombreCompleto { get; private set; }
    public RolUsuario? Rol { get; private set; }
    public int? InventarioOperativoId { get; private set; }

    public bool HaySesionActiva => !string.IsNullOrEmpty(Token) && InventarioOperativoId is not null;

    public async Task IniciarSesionAsync(LoginResponse respuesta)
    {
        Token = respuesta.Token;
        UsuarioId = respuesta.UsuarioId;
        NombreUsuario = respuesta.NombreUsuario;
        NombreCompleto = respuesta.NombreCompleto;
        Rol = respuesta.Rol;
        InventarioOperativoId = null;

        // El token va en SecureStorage (cifrado por el SO); el resto son datos no sensibles en Preferences.
        await SecureStorage.Default.SetAsync(ClaveToken, Token);
        Preferences.Default.Set(ClaveUsuarioId, UsuarioId.Value);
        Preferences.Default.Set(ClaveNombreUsuario, NombreUsuario);
        Preferences.Default.Set(ClaveNombreCompleto, NombreCompleto ?? string.Empty);
        Preferences.Default.Set(ClaveRol, Rol.Value.ToString());
        Preferences.Default.Remove(ClaveInventarioId);
    }

    public Task FijarInventarioOperativoAsync(int inventarioId)
    {
        InventarioOperativoId = inventarioId;
        Preferences.Default.Set(ClaveInventarioId, inventarioId);
        return Task.CompletedTask;
    }

    public async Task<bool> RestaurarSesionAsync()
    {
        try
        {
            Token = await SecureStorage.Default.GetAsync(ClaveToken);
        }
        catch (Exception)
        {
            // Algunas plataformas invalidan SecureStorage si cambia la firma/keystore de la app entre builds.
            Token = null;
        }

        if (string.IsNullOrEmpty(Token) || !Preferences.Default.ContainsKey(ClaveUsuarioId))
        {
            return false;
        }

        UsuarioId = Preferences.Default.Get(ClaveUsuarioId, 0);
        NombreUsuario = Preferences.Default.Get(ClaveNombreUsuario, string.Empty);
        NombreCompleto = Preferences.Default.Get(ClaveNombreCompleto, string.Empty);
        Rol = Enum.TryParse<RolUsuario>(Preferences.Default.Get(ClaveRol, string.Empty), out var rol) ? rol : null;
        InventarioOperativoId = Preferences.Default.ContainsKey(ClaveInventarioId)
            ? Preferences.Default.Get(ClaveInventarioId, 0)
            : null;

        // Nota: no se valida aquí si el JWT ya expiró; la primera llamada a la API que devuelva 401
        // hace que BaseViewModel cierre la sesión y regrese a Login (ver BaseViewModel.EjecutarAsync).
        return Rol is not null;
    }

    public Task CerrarSesionAsync()
    {
        Token = null;
        UsuarioId = null;
        NombreUsuario = null;
        NombreCompleto = null;
        Rol = null;
        InventarioOperativoId = null;

        SecureStorage.Default.Remove(ClaveToken);
        Preferences.Default.Clear();

        return Task.CompletedTask;
    }
}
