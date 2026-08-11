using Inventario.Core.Entities;
using Inventario.Core.Interfaces;
using Inventario.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly InventarioDbContext _context;

    public UsuarioRepository(InventarioDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> ObtenerPorIdAsync(int id)
    {
        return await _context.Usuarios
            .Include(u => u.Sucursal)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario)
    {
        return await _context.Usuarios
            .Include(u => u.Sucursal)
            .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);
    }

    public async Task<IEnumerable<Usuario>> ObtenerTodosAsync()
    {
        return await _context.Usuarios
            .Where(u => u.Activo)
            .OrderBy(u => u.NombreUsuario)
            .ToListAsync();
    }

    public async Task AgregarAsync(Usuario usuario)
    {
        await _context.Usuarios.AddAsync(usuario);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarAsync(Usuario usuario)
    {
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExisteAlgunoAsync() => await _context.Usuarios.AnyAsync();
}
