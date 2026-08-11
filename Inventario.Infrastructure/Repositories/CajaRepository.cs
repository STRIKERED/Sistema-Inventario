using Inventario.Core.Entities;
using Inventario.Core.Interfaces;
using Inventario.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infrastructure.Repositories;

public class CajaRepository : ICajaRepository
{
    private readonly InventarioDbContext _context;

    public CajaRepository(InventarioDbContext context)
    {
        _context = context;
    }

    public async Task<Caja?> ObtenerPorIdAsync(int id)
    {
        return await _context.Cajas
            .Include(c => c.Sucursal)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Caja>> ObtenerPorSucursalAsync(int sucursalId)
    {
        return await _context.Cajas
            .Where(c => c.SucursalId == sucursalId)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    public async Task AgregarAsync(Caja caja)
    {
        await _context.Cajas.AddAsync(caja);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarAsync(Caja caja)
    {
        _context.Cajas.Update(caja);
        await _context.SaveChangesAsync();
    }
}
