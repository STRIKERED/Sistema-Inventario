using System.ComponentModel.DataAnnotations;
using Inventario.Core.Enums;

namespace Inventario.Core.Dtos;

// Excluye PasswordHash a propósito: nunca debe exponerse vía API, ni siquiera hasheado.
public record UsuarioDto(
    int Id,
    string NombreUsuario,
    string? NombreCompleto,
    RolUsuario Rol,
    bool Activo,
    int? SucursalId);

public record CrearUsuarioRequest(
    [Required, StringLength(50)] string NombreUsuario,
    [Required, MinLength(6)] string Password,
    string? NombreCompleto,
    RolUsuario Rol,
    int? SucursalId);

// No incluye la contraseña: para cambiarla haría falta un endpoint dedicado que la vuelva a hashear.
public record ActualizarUsuarioRequest(
    [Required, StringLength(50)] string NombreUsuario,
    string? NombreCompleto,
    RolUsuario Rol,
    bool Activo,
    int? SucursalId);
