using Inventario.Core.Dtos;
using Inventario.Core.Entities;

namespace Inventario.Core.Mapping;

public static class UsuarioMapping
{
    // Nunca incluye PasswordHash: se excluye a propósito en UsuarioDto.
    // Requiere que UsuarioInventarios.Inventario venga cargado (ver UsuarioRepository) para armar la lista.
    public static UsuarioDto ToDto(this Usuario usuario) =>
        new(usuario.Id, usuario.NombreUsuario, usuario.NombreCompleto, usuario.Rol, usuario.Activo,
            usuario.UsuarioInventarios
                .Where(ui => ui.Inventario is not null)
                .Select(ui => ui.Inventario!.ToDto())
                .ToList());

    public static IEnumerable<UsuarioDto> ToDto(this IEnumerable<Usuario> usuarios) =>
        usuarios.Select(u => u.ToDto());

    // El hash de la contraseña se calcula en el controller (necesita IPasswordHasher), no aquí.
    // InventarioIds no se aplica aquí: el controller sincroniza UsuarioInventario aparte, después de
    // insertar (necesita el Id del usuario ya generado) — ver IUsuarioRepository.SincronizarInventariosAsync.
    public static Usuario ToEntity(this CrearUsuarioRequest request, string passwordHash) =>
        new()
        {
            NombreUsuario = request.NombreUsuario,
            PasswordHash = passwordHash,
            NombreCompleto = request.NombreCompleto,
            Rol = request.Rol,
            Activo = true
        };

    public static void AplicarA(this ActualizarUsuarioRequest request, Usuario usuario)
    {
        usuario.NombreUsuario = request.NombreUsuario;
        usuario.NombreCompleto = request.NombreCompleto;
        usuario.Rol = request.Rol;
        usuario.Activo = request.Activo;
    }
}
