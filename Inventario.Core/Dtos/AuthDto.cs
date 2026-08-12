using System.ComponentModel.DataAnnotations;
using Inventario.Core.Enums;

namespace Inventario.Core.Dtos;

public record LoginRequest(
    [Required] string NombreUsuario,
    [Required] string Password);

public record LoginResponse(
    string Token,
    int UsuarioId,
    string NombreUsuario,
    string? NombreCompleto,
    RolUsuario Rol,
    int? SucursalId);

/// <summary>Le dice al cliente si ya hay al menos un usuario dado de alta (para decidir si mostrar el
/// login normal o el formulario de "crear el primer administrador").</summary>
public record EstadoSistemaResponse(bool HayUsuarios);

// Sin Rol ni SucursalId: el primer usuario del sistema siempre se crea como Administrador y sin
// sucursal fija (la elige él mismo tras iniciar sesión, como cualquier Administrador).
public record RegistrarUsuarioInicialRequest(
    [Required, StringLength(50)] string NombreUsuario,
    [Required, MinLength(6)] string Password,
    string? NombreCompleto);
