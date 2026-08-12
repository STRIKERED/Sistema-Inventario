using Inventario.Core.Interfaces;
using Inventario.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using InventarioEntity = Inventario.Core.Entities.Inventario;

namespace Inventario.Infrastructure.Repositories;

public class InventarioRepository : IInventarioRepository
{
    private readonly InventarioDbContext _context;

    public InventarioRepository(InventarioDbContext context)
    {
        _context = context;
    }

    public async Task<InventarioEntity?> ObtenerPorIdAsync(int id)
    {
        return await _context.Inventarios
            .Include(i => i.Sucursal)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<IEnumerable<InventarioEntity>> ObtenerTodosAsync()
    {
        return await _context.Inventarios
            .Include(i => i.Sucursal)
            .Where(i => i.Activo)
            .OrderBy(i => i.Nombre)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventarioEntity>> ObtenerPorSucursalAsync(int sucursalId)
    {
        return await _context.Inventarios
            .Include(i => i.Sucursal)
            .Where(i => i.SucursalId == sucursalId)
            .OrderBy(i => i.Nombre)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventarioEntity>> ObtenerAsignadosAUsuarioAsync(int usuarioId)
    {
        return await _context.Inventarios
            .Include(i => i.Sucursal)
            .Where(i => i.Activo && i.UsuarioInventarios.Any(ui => ui.UsuarioId == usuarioId))
            .OrderBy(i => i.Nombre)
            .ToListAsync();
    }

    public async Task AgregarAsync(InventarioEntity inventario)
    {
        await _context.Inventarios.AddAsync(inventario);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarAsync(InventarioEntity inventario)
    {
        _context.Inventarios.Update(inventario);
        await _context.SaveChangesAsync();
    }
}
