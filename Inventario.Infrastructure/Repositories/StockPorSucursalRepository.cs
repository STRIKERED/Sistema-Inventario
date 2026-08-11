using Inventario.Core.Entities;
using Inventario.Core.Interfaces;
using Inventario.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infrastructure.Repositories;

public class StockPorSucursalRepository : IStockPorSucursalRepository
{
    private readonly InventarioDbContext _context;

    public StockPorSucursalRepository(InventarioDbContext context)
    {
        _context = context;
    }

    public async Task<StockPorSucursal?> ObtenerAsync(int productoId, int sucursalId)
    {
        return await _context.StockPorSucursal
            .FirstOrDefaultAsync(s => s.ProductoId == productoId && s.SucursalId == sucursalId);
    }

    public async Task<IEnumerable<StockPorSucursal>> ObtenerPorSucursalAsync(int sucursalId)
    {
        return await _context.StockPorSucursal
            .Include(s => s.Producto)
            .Where(s => s.SucursalId == sucursalId)
            .ToListAsync();
    }

    public async Task<IEnumerable<StockPorSucursal>> ObtenerPorProductoAsync(int productoId)
    {
        return await _context.StockPorSucursal
            .Include(s => s.Sucursal)
            .Where(s => s.ProductoId == productoId)
            .ToListAsync();
    }

    public async Task AgregarAsync(StockPorSucursal stock)
    {
        await _context.StockPorSucursal.AddAsync(stock);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarAsync(StockPorSucursal stock)
    {
        _context.StockPorSucursal.Update(stock);
        await _context.SaveChangesAsync();
    }
}
