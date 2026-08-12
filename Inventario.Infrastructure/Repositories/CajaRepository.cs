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
            .Include(c => c.Inventario)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Caja>> ObtenerPorInventarioAsync(int inventarioId)
    {
        return await _context.Cajas
            .Include(c => c.Inventario)
            .Where(c => c.InventarioId == inventarioId)
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
