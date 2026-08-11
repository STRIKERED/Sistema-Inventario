using System.ComponentModel.DataAnnotations;
using Inventario.Core.Enums;

namespace Inventario.Core.Dtos;

public record LoginRequest(
    [property: Required] string NombreUsuario,
    [property: Required] string Password);

public record LoginResponse(
    string Token,
    int UsuarioId,
    string NombreUsuario,
    string? NombreCompleto,
    RolUsuario Rol,
    int? SucursalId);
