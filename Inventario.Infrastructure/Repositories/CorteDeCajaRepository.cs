using Inventario.Core.Entities;
using Inventario.Core.Enums;
using Inventario.Core.Interfaces;
using Inventario.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infrastructure.Repositories;

public class CorteDeCajaRepository : ICorteDeCajaRepository
{
    private readonly InventarioDbContext _context;

    public CorteDeCajaRepository(InventarioDbContext context)
    {
        _context = context;
    }

    public async Task<CorteDeCaja?> ObtenerPorIdAsync(int id)
    {
        return await _context.CortesDeCaja
            .Include(c => c.Caja)
            .Include(c => c.Usuario)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<CorteDeCaja?> ObtenerAbiertoPorCajaAsync(int cajaId)
    {
        return await _context.CortesDeCaja
            .Where(c => c.CajaId == cajaId && c.Estado == EstadoCorteDeCaja.Abierto)
            .OrderByDescending(c => c.FechaApertura)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<CorteDeCaja>> ObtenerPorCajaAsync(int cajaId)
    {
        return await _context.CortesDeCaja
            .Where(c => c.CajaId == cajaId)
            .OrderByDescending(c => c.FechaApertura)
            .ToListAsync();
    }

    public async Task<CorteDeCaja> CrearAsync(CorteDeCaja corteDeCaja)
    {
        await _context.CortesDeCaja.AddAsync(corteDeCaja);
        await _context.SaveChangesAsync();
        return corteDeCaja;
    }

    public async Task ActualizarAsync(CorteDeCaja corteDeCaja)
    {
        _context.CortesDeCaja.Update(corteDeCaja);
        await _context.SaveChangesAsync();
    }
}
