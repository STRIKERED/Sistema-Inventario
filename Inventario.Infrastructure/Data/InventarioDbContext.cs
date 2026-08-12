using Inventario.Core.Entities;
using Microsoft.EntityFrameworkCore;
using InventarioEntity = Inventario.Core.Entities.Inventario;

namespace Inventario.Infrastructure.Data;

public class InventarioDbContext : DbContext
{
    public InventarioDbContext(DbContextOptions<InventarioDbContext> options)
        : base(options)
    {
    }

    public DbSet<Sucursal> Sucursales => Set<Sucursal>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<InventarioEntity> Inventarios => Set<InventarioEntity>();
    public DbSet<UsuarioInventario> UsuariosInventarios => Set<UsuarioInventario>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Caja> Cajas => Set<Caja>();
    public DbSet<CorteDeCaja> CortesDeCaja => Set<CorteDeCaja>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();
    public DbSet<Cotizacion> Cotizaciones => Set<Cotizacion>();
    public DbSet<DetalleCotizacion> DetallesCotizacion => Set<DetalleCotizacion>();
    public DbSet<MovimientoInventario> MovimientosInventario => Set<MovimientoInventario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventarioDbContext).Assembly);
    }
}
