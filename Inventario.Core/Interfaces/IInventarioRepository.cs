using InventarioEntity = Inventario.Core.Entities.Inventario;

namespace Inventario.Core.Interfaces;

public interface IInventarioRepository
{
    Task<InventarioEntity?> ObtenerPorIdAsync(int id);

    /// <summary>Todos los inventarios activos (usado para el acceso implícito de Administrador).</summary>
    Task<IEnumerable<InventarioEntity>> ObtenerTodosAsync();

    Task<IEnumerable<InventarioEntity>> ObtenerPorSucursalAsync(int sucursalId);

    /// <summary>Inventarios activos a los que un Usuario tiene acceso explícito vía UsuarioInventario.</summary>
    Task<IEnumerable<InventarioEntity>> ObtenerAsignadosAUsuarioAsync(int usuarioId);

    Task AgregarAsync(InventarioEntity inventario);
    Task ActualizarAsync(InventarioEntity inventario);
}
