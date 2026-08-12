using System.ComponentModel.DataAnnotations;
using Inventario.Core.Enums;

namespace Inventario.Core.Dtos;

public record LoginRequest(
    [Required] string NombreUsuario,
    [Required] string Password);

// Inventarios: los accesibles para este usuario (Administrador -> todos los activos; el resto ->
// sus UsuarioInventario). El cliente los usa para armar el selector sin un round-trip aparte, y
// auto-selecciona si viene exactamente uno.
public record LoginResponse(
    string Token,
    int UsuarioId,
    string NombreUsuario,
    string? NombreCompleto,
    RolUsuario Rol,
    IReadOnlyList<InventarioDto> Inventarios);

/// <summary>Le dice al cliente si ya hay al menos un usuario dado de alta (para decidir si mostrar el
/// login normal o el formulario de "crear el primer administrador").</summary>
public record EstadoSistemaResponse(bool HayUsuarios);

// Sin Rol ni InventarioIds: el primer usuario del sistema siempre se crea como Administrador, que
// tiene acceso implícito a todos los Inventarios activos (no necesita fila en UsuarioInventario).
public record RegistrarUsuarioInicialRequest(
    [Required, StringLength(50)] string NombreUsuario,
    [Required, MinLength(6)] string Password,
    string? NombreCompleto);
