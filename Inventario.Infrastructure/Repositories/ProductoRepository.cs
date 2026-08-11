using Inventario.Core.Entities;
using Inventario.Core.Interfaces;
using Inventario.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infrastructure.Repositories;

public class ProductoRepository : IProductoRepository
{
    private readonly InventarioDbContext _context;

    public ProductoRepository(InventarioDbContext context)
    {
        _context = context;
    }

    public async Task<Producto?> ObtenerPorIdAsync(int id)
    {
        return await _context.Productos
            .Include(p => p.Stocks)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Producto?> ObtenerPorCodigoBarrasAsync(string codigoBarras)
    {
        return await _context.Productos
            .Include(p => p.Stocks)
            .FirstOrDefaultAsync(p => p.CodigoBarras == codigoBarras);
    }

    public async Task<IEnumerable<Producto>> ObtenerTodosAsync()
    {
        return await _context.Productos
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    public async Task AgregarAsync(Producto producto)
    {
        await _context.Productos.AddAsync(producto);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarAsync(Producto producto)
    {
        _context.Productos.Update(producto);
        await _context.SaveChangesAsync();
    }
}
