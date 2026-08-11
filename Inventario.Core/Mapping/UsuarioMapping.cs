using Inventario.Core.Dtos;
using Inventario.Core.Entities;

namespace Inventario.Core.Mapping;

public static class UsuarioMapping
{
    // Nunca incluye PasswordHash: se excluye a propósito en UsuarioDto.
    public static UsuarioDto ToDto(this Usuario usuario) =>
        new(usuario.Id, usuario.NombreUsuario, usuario.NombreCompleto, usuario.Rol, usuario.Activo, usuario.SucursalId);

    public static IEnumerable<UsuarioDto> ToDto(this IEnumerable<Usuario> usuarios) =>
        usuarios.Select(u => u.ToDto());

    // El hash de la contraseña se calcula en el controller (necesita IPasswordHasher), no aquí.
    public static Usuario ToEntity(this CrearUsuarioRequest request, string passwordHash) =>
        new()
        {
            NombreUsuario = request.NombreUsuario,
            PasswordHash = passwordHash,
            NombreCompleto = request.NombreCompleto,
            Rol = request.Rol,
            SucursalId = request.SucursalId,
            Activo = true
        };

    public static void AplicarA(this ActualizarUsuarioRequest request, Usuario usuario)
    {
        usuario.NombreUsuario = request.NombreUsuario;
        usuario.NombreCompleto = request.NombreCompleto;
        usuario.Rol = request.Rol;
        usuario.Activo = request.Activo;
        usuario.SucursalId = request.SucursalId;
    }
}
