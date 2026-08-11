using Inventario.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infrastructure.Data;

public class InventarioDbContext : DbContext
{
    public InventarioDbContext(DbContextOptions<InventarioDbContext> options)
        : base(options)
    {
    }

    public DbSet<Sucursal> Sucursales => Set<Sucursal>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<StockPorSucursal> StockPorSucursal => Set<StockPorSucursal>();
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

        modelBuilder.Entity<Sucursal>(entity =>
        {
            entity.Property(s => s.Nombre).HasMaxLength(300).IsRequired();
            entity.Property(s => s.Direccion).HasMaxLength(600);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.Property(u => u.NombreUsuario).HasMaxLength(100).IsRequired();
            entity.Property(u => u.NombreCompleto).HasMaxLength(300);
            entity.HasIndex(u => u.NombreUsuario).IsUnique();
            entity.HasOne(u => u.Sucursal)
                .WithMany()
                .HasForeignKey(u => u.SucursalId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.Property(p => p.Sku).HasMaxLength(100).IsRequired();
            entity.Property(p => p.CodigoBarras).HasMaxLength(100).IsRequired();
            entity.Property(p => p.Nombre).HasMaxLength(400).IsRequired();
            entity.Property(p => p.Categoria).HasMaxLength(200);
            entity.Property(p => p.Unidad).HasMaxLength(40);
            entity.Property(p => p.PrecioCosto).HasColumnType("decimal(18,2)");
            entity.Property(p => p.PrecioVenta).HasColumnType("decimal(18,2)");
            entity.Property(p => p.Activo).HasDefaultValue(true);
            entity.HasIndex(p => p.Sku).IsUnique();
            entity.HasIndex(p => p.CodigoBarras).IsUnique();
        });

        modelBuilder.Entity<StockPorSucursal>(entity =>
        {
            entity.HasIndex(s => new { s.ProductoId, s.SucursalId }).IsUnique();
            entity.HasOne(s => s.Producto)
                .WithMany(p => p.Stocks)
                .HasForeignKey(s => s.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(s => s.Sucursal)
                .WithMany()
                .HasForeignKey(s => s.SucursalId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Caja>(entity =>
        {
            entity.Property(c => c.Nombre).HasMaxLength(100).IsRequired();
            entity.HasOne(c => c.Sucursal)
                .WithMany()
                .HasForeignKey(c => c.SucursalId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CorteDeCaja>(entity =>
        {
            entity.Property(c => c.MontoInicial).HasColumnType("decimal(18,2)");
            entity.Property(c => c.MontoFinalContado).HasColumnType("decimal(18,2)");
            entity.Property(c => c.MontoFinalSistema).HasColumnType("decimal(18,2)");
            entity.Property(c => c.Diferencia).HasColumnType("decimal(18,2)");
            entity.HasOne(c => c.Caja)
                .WithMany()
                .HasForeignKey(c => c.CajaId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(c => c.Usuario)
                .WithMany()
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.Property(v => v.Folio).HasMaxLength(40).IsRequired();
            entity.Property(v => v.Subtotal).HasColumnType("decimal(18,2)");
            entity.Property(v => v.Descuento).HasColumnType("decimal(18,2)");
            entity.Property(v => v.Impuestos).HasColumnType("decimal(18,2)");
            entity.Property(v => v.Total).HasColumnType("decimal(18,2)");
            entity.Property(v => v.Cancelada).HasDefaultValue(false);
            entity.HasIndex(v => v.Folio).IsUnique();
            entity.HasOne(v => v.Sucursal)
                .WithMany()
                .HasForeignKey(v => v.SucursalId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(v => v.CorteDeCaja)
                .WithMany()
                .HasForeignKey(v => v.CorteDeCajaId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(v => v.Usuario)
                .WithMany()
                .HasForeignKey(v => v.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DetalleVenta>(entity =>
        {
            entity.Property(d => d.PrecioUnitario).HasColumnType("decimal(18,2)");
            entity.Property(d => d.DescuentoUnitario).HasColumnType("decimal(18,2)");
            entity.HasOne(d => d.Venta)
                .WithMany(v => v.Detalles)
                .HasForeignKey(d => d.VentaId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(d => d.Producto)
                .WithMany()
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Cotizacion>(entity =>
        {
            entity.Property(c => c.Folio).HasMaxLength(40).IsRequired();
            entity.Property(c => c.ClienteNombre).HasMaxLength(400);
            entity.Property(c => c.ClienteContacto).HasMaxLength(300);
            entity.Property(c => c.FechaCreacion).HasColumnName("Fecha");
            entity.Property(c => c.Subtotal).HasColumnType("decimal(18,2)");
            entity.Property(c => c.Descuento).HasColumnType("decimal(18,2)");
            entity.Property(c => c.Impuestos).HasColumnType("decimal(18,2)");
            entity.Property(c => c.Total).HasColumnType("decimal(18,2)");
            entity.HasIndex(c => c.Folio).IsUnique();
            entity.HasOne(c => c.Sucursal)
                .WithMany()
                .HasForeignKey(c => c.SucursalId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(c => c.Usuario)
                .WithMany()
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DetalleCotizacion>(entity =>
        {
            entity.Property(d => d.PrecioUnitario).HasColumnType("decimal(18,2)");
            entity.HasOne(d => d.Cotizacion)
                .WithMany(c => c.Detalles)
                .HasForeignKey(d => d.CotizacionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(d => d.Producto)
                .WithMany()
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MovimientoInventario>(entity =>
        {
            entity.Property(m => m.Motivo).HasMaxLength(600);
            entity.HasOne(m => m.Producto)
                .WithMany()
                .HasForeignKey(m => m.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(m => m.Sucursal)
                .WithMany()
                .HasForeignKey(m => m.SucursalId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(m => m.Usuario)
                .WithMany()
                .HasForeignKey(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
