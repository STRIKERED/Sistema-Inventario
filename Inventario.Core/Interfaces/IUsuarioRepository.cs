using Inventario.Core.Entities;

namespace Inventario.Core.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorIdAsync(int id);
    Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario);
    Task<IEnumerable<Usuario>> ObtenerTodosAsync();
    Task AgregarAsync(Usuario usuario);
    Task ActualizarAsync(Usuario usuario);
}
