using Inventario.Core.Entities;
using Inventario.Core.Interfaces;
using Inventario.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infrastructure.Repositories;

public class MovimientoInventarioRepository : IMovimientoInventarioRepository
{
    private readonly InventarioDbContext _context;

    public MovimientoInventarioRepository(InventarioDbContext context)
    {
        _context = context;
    }

    public async Task<MovimientoInventario?> ObtenerPorIdAsync(int id)
    {
        return await _context.MovimientosInventario
            .Include(m => m.Producto)
            .Include(m => m.Usuario)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<MovimientoInventario>> ObtenerPorProductoAsync(int productoId)
    {
        return await _context.MovimientosInventario
            .Include(m => m.Usuario)
            .Where(m => m.ProductoId == productoId)
            .OrderByDescending(m => m.Fecha)
            .ToListAsync();
    }

    public async Task<MovimientoInventario> CrearAsync(MovimientoInventario movimiento)
    {
        await _context.MovimientosInventario.AddAsync(movimiento);
        await _context.SaveChangesAsync();
        return movimiento;
    }
}
