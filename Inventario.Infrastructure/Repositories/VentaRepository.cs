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
            .Include(v => v.Inventario)
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

    public async Task<IEnumerable<Venta>> ObtenerPorInventarioAsync(
        int inventarioId, DateTime desde, DateTime hasta, bool cancelada = false)
    {
        return await _context.Ventas
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .Include(v => v.Usuario)
            .Where(v => v.InventarioId == inventarioId && v.Cancelada == cancelada && v.Fecha >= desde && v.Fecha <= hasta)
            .OrderByDescending(v => v.Fecha)
            .ToListAsync();
    }

    public async Task ActualizarFolioAsync(int id, string folio)
    {
        // FindAsync reutiliza la instancia ya trackeada por el DbContext (misma request/scope) si la venta
        // se acaba de insertar con CrearAsync, así que esto no dispara una segunda consulta a la BD.
        var venta = await _context.Ventas.FindAsync(id);
        if (venta is null)
        {
            return;
        }

        venta.Folio = folio;
        await _context.SaveChangesAsync();
    }
}
