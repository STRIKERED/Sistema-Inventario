using System.Security.Claims;
using System.Text.Json;
using Inventario.Core.Dtos;
using Inventario.Core.Enums;
using Microsoft.AspNetCore.Http;

namespace Inventario.Web.Services.Sesion;

public class CurrentSessionAccessor : ICurrentSessionAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentSessionAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // Se lee en vivo (no se cachea un ClaimsPrincipal en el constructor): ISesionAuthService puede
    // firmar una cookie nueva a mitad de la misma request (login, cambio de Inventario), y
    // HttpContext.User ya queda actualizado a partir de ese SignInAsync — este accessor debe verlo.
    private ClaimsPrincipal? Usuario => _httpContextAccessor.HttpContext?.User;

    public bool HaySesionActiva => Usuario?.Identity?.IsAuthenticated == true;

    public string? Token => ObtenerClaim(InventarioClaimTypes.Token);
    public int? UsuarioId => int.TryParse(ObtenerClaim(ClaimTypes.NameIdentifier), out var id) ? id : null;
    public string? NombreUsuario => ObtenerClaim(ClaimTypes.Name);
    public string? NombreCompleto => ObtenerClaim(InventarioClaimTypes.NombreCompleto) is { Length: > 0 } nombre ? nombre : null;

    public RolUsuario? Rol =>
        Enum.TryParse<RolUsuario>(ObtenerClaim(ClaimTypes.Role), out var rol) ? rol : null;

    public int? InventarioOperativoId =>
        int.TryParse(ObtenerClaim(InventarioClaimTypes.InventarioId), out var id) ? id : null;

    public string? InventarioOperativoNombre =>
        InventariosDisponibles.FirstOrDefault(i => i.Id == InventarioOperativoId)?.Nombre;

    public IReadOnlyList<InventarioDto> InventariosDisponibles
    {
        get
        {
            var json = ObtenerClaim(InventarioClaimTypes.InventariosDisponibles);
            if (string.IsNullOrWhiteSpace(json))
            {
                return [];
            }

            return JsonSerializer.Deserialize<List<InventarioDto>>(json) ?? [];
        }
    }

    public bool DebeSeleccionarInventario => HaySesionActiva && InventarioOperativoId is null;

    private string? ObtenerClaim(string tipo) => Usuario?.FindFirstValue(tipo);
}
