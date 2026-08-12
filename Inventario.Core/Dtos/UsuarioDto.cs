using System.ComponentModel.DataAnnotations;
using Inventario.Core.Enums;

namespace Inventario.Core.Dtos;

// Excluye PasswordHash a propósito: nunca debe exponerse vía API, ni siquiera hasheado.
// Administrador no trae Inventarios aquí (tiene acceso implícito a todos, ver AuthController);
// para el resto de los roles, es la lista de inventarios asignados vía UsuarioInventario.
public record UsuarioDto(
    int Id,
    string NombreUsuario,
    string? NombreCompleto,
    RolUsuario Rol,
    bool Activo,
    IReadOnlyList<InventarioDto> Inventarios);

public record CrearUsuarioRequest(
    [Required, StringLength(50)] string NombreUsuario,
    [Required, MinLength(6)] string Password,
    string? NombreCompleto,
    RolUsuario Rol,
    IReadOnlyList<int> InventarioIds);

// No incluye la contraseña: para cambiarla haría falta un endpoint dedicado que la vuelva a hashear.
public record ActualizarUsuarioRequest(
    [Required, StringLength(50)] string NombreUsuario,
    string? NombreCompleto,
    RolUsuario Rol,
    bool Activo,
    IReadOnlyList<int> InventarioIds);
