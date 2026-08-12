using Inventario.Core.Entities;
using Inventario.Core.Enums;
using Inventario.Core.Interfaces;
using Inventario.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infrastructure.Repositories;

public class CotizacionRepository : ICotizacionRepository
{
    private readonly InventarioDbContext _context;

    public CotizacionRepository(InventarioDbContext context)
    {
        _context = context;
    }

    public async Task<Cotizacion?> ObtenerPorIdAsync(int id)
    {
        return await _context.Cotizaciones
            .Include(c => c.Detalles)
                .ThenInclude(d => d.Producto)
            .Include(c => c.Usuario)
            .Include(c => c.Inventario)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Cotizacion> CrearAsync(Cotizacion cotizacion)
    {
        await _context.Cotizaciones.AddAsync(cotizacion);
        await _context.SaveChangesAsync();
        return cotizacion;
    }

    public async Task<IEnumerable<Cotizacion>> ObtenerVigentesAsync(int inventarioId)
    {
        return await _context.Cotizaciones
            .Include(c => c.Detalles)
            .Where(c => c.InventarioId == inventarioId
                     && c.Estado == EstadoCotizacion.Vigente
                     && c.FechaVigencia >= DateTime.UtcNow)
            .OrderByDescending(c => c.FechaCreacion)
            .ToListAsync();
    }

    public async Task ActualizarAsync(Cotizacion cotizacion)
    {
        _context.Cotizaciones.Update(cotizacion);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarFolioAsync(int id, string folio)
    {
        var cotizacion = await _context.Cotizaciones.FindAsync(id);
        if (cotizacion is null)
        {
            return;
        }

        cotizacion.Folio = folio;
        await _context.SaveChangesAsync();
    }
}
