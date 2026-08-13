using Inventario.Core.Entities;
using Inventario.Core.Interfaces;
using Inventario.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infrastructure.Repositories;

public class ConfiguracionImpresionRepository : IConfiguracionImpresionRepository
{
    private readonly InventarioDbContext _context;

    public ConfiguracionImpresionRepository(InventarioDbContext context)
    {
        _context = context;
    }

    public async Task<ConfiguracionImpresion?> ObtenerPorIdAsync(int id)
    {
        return await _context.ConfiguracionesImpresion
            .Include(c => c.Inventario)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<ConfiguracionImpresion?> ObtenerPorInventarioAsync(int inventarioId)
    {
        return await _context.ConfiguracionesImpresion
            .Include(c => c.Inventario)
            .FirstOrDefaultAsync(c => c.InventarioId == inventarioId);
    }

    public async Task AgregarAsync(ConfiguracionImpresion configuracion)
    {
        await _context.ConfiguracionesImpresion.AddAsync(configuracion);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarAsync(ConfiguracionImpresion configuracion)
    {
        _context.ConfiguracionesImpresion.Update(configuracion);
        await _context.SaveChangesAsync();
    }
}
