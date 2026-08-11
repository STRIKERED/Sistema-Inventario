using Inventario.Core.Entities;
using Inventario.Core.Interfaces;
using Inventario.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infrastructure.Repositories;

public class SucursalRepository : ISucursalRepository
{
    private readonly InventarioDbContext _context;

    public SucursalRepository(InventarioDbContext context)
    {
        _context = context;
    }

    public async Task<Sucursal?> ObtenerPorIdAsync(int id)
    {
        return await _context.Sucursales.FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<Sucursal>> ObtenerTodasAsync()
    {
        return await _context.Sucursales
            .OrderBy(s => s.Nombre)
            .ToListAsync();
    }

    public async Task AgregarAsync(Sucursal sucursal)
    {
        await _context.Sucursales.AddAsync(sucursal);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarAsync(Sucursal sucursal)
    {
        _context.Sucursales.Update(sucursal);
        await _context.SaveChangesAsync();
    }
}
