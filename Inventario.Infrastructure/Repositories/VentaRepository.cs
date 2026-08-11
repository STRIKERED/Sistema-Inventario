using Inventario.Core.Entities;
using Inventario.Core.Interfaces;
using Inventario.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infrastructure.Repositories;

public class VentaRepository : IVentaRepository
{
    private readonly InventarioDbContext _context;

    public VentaRepository(InventarioDbContext context)
    {
        _context = context;
    }

    public async Task<Venta?> ObtenerPorIdAsync(int id)
    {
        return await _context.Ventas
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .Include(v => v.Usuario)
            .Include(v => v.Sucursal)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<Venta> CrearAsync(Venta venta)
    {
        await _context.Ventas.AddAsync(venta);
        await _context.SaveChangesAsync();
        return venta;
    }

    public async Task<IEnumerable<Venta>> ObtenerPorCorteDeCajaAsync(int corteDeCajaId)
    {
        return await _context.Ventas
            .Include(v => v.Detalles)
            .Where(v => v.CorteDeCajaId == corteDeCajaId && !v.Cancelada)
            .OrderBy(v => v.Fecha)
            .ToListAsync();
    }
}
