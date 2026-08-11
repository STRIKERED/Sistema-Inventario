using Inventario.Core.Entities;

namespace Inventario.Core.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorIdAsync(int id);
    Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario);
    Task<IEnumerable<Usuario>> ObtenerTodosAsync();
    Task AgregarAsync(Usuario usuario);
    Task ActualizarAsync(Usuario usuario);

    /// <summary>true si existe al menos un usuario en el sistema (sin importar Activo). Se usa para
    /// decidir si habilitar el registro del primer Administrador sin autenticación.</summary>
    Task<bool> ExisteAlgunoAsync();
}
