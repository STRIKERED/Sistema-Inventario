using System.Security.Claims;
using System.Text.Json;
using Inventario.Core.Dtos;
using Inventario.Core.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Inventario.Web.Services.Sesion;

public class SesionAuthService : ISesionAuthService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICurrentSessionAccessor _sesionActual;

    public SesionAuthService(IHttpContextAccessor httpContextAccessor, ICurrentSessionAccessor sesionActual)
    {
        _httpContextAccessor = httpContextAccessor;
        _sesionActual = sesionActual;
    }

    private HttpContext HttpContext =>
        _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("No hay HttpContext activo.");

    public Task IniciarSesionAsync(LoginResponse respuesta)
    {
        var inventarioId = respuesta.Inventarios.Count == 1 ? respuesta.Inventarios[0].Id : (int?)null;

        return FirmarAsync(
            respuesta.Token, respuesta.UsuarioId, respuesta.NombreUsuario, respuesta.NombreCompleto,
            respuesta.Rol, respuesta.Inventarios, inventarioId);
    }

    public async Task<bool> FijarInventarioOperativoAsync(int inventarioId)
    {
        var disponibles = _sesionActual.InventariosDisponibles;
        if (disponibles.All(i => i.Id != inventarioId))
        {
            return false;
        }

        await FirmarAsync(
            _sesionActual.Token!, _sesionActual.UsuarioId!.Value, _sesionActual.NombreUsuario!,
            _sesionActual.NombreCompleto, _sesionActual.Rol!.Value, disponibles, inventarioId);
        return true;
    }

    public Task CerrarSesionAsync() =>
        HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

    private Task FirmarAsync(
        string token, int usuarioId, string nombreUsuario, string? nombreCompleto,
        RolUsuario rol, IReadOnlyList<InventarioDto> inventarios, int? inventarioId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuarioId.ToString()),
            new(ClaimTypes.Name, nombreUsuario),
            new(ClaimTypes.Role, rol.ToString()),
            new(InventarioClaimTypes.Token, token),
            new(InventarioClaimTypes.NombreCompleto, nombreCompleto ?? string.Empty),
            new(InventarioClaimTypes.InventariosDisponibles, JsonSerializer.Serialize(inventarios))
        };

        if (inventarioId is not null)
        {
            claims.Add(new Claim(InventarioClaimTypes.InventarioId, inventarioId.Value.ToString()));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        // IsPersistent + ExpiresUtc: la cookie sobrevive a cerrar el navegador durante el mismo plazo
        // que dura el JWT (ver options.ExpireTimeSpan en Program.cs); no hay refresh token, así que al
        // expirar el JWT la siguiente llamada a la Api responde 401 y el filtro global cierra la sesión.
        return HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = true });
    }
}
