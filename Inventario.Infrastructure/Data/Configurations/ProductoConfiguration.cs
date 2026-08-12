using Inventario.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Infrastructure.Data.Configurations;

public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> entity)
    {
        entity.Property(p => p.Sku).HasMaxLength(100).IsRequired();
        entity.Property(p => p.CodigoBarras).HasMaxLength(100).IsRequired();
        entity.Property(p => p.Nombre).HasMaxLength(400).IsRequired();
        entity.Property(p => p.Categoria).HasMaxLength(200);
        entity.Property(p => p.Unidad).HasMaxLength(40);
        entity.Property(p => p.Activo).HasDefaultValue(true);
        entity.Property(p => p.CantidadDisponible).HasDefaultValue(0);
        entity.Property(p => p.StockMinimo).HasDefaultValue(0);

        // Únicos por Inventario (no globales): dos inventarios distintos pueden repetir Sku/CodigoBarras,
        // cada uno con su propio catálogo y stock independiente.
        entity.HasIndex(p => new { p.InventarioId, p.Sku }).IsUnique();
        entity.HasIndex(p => new { p.InventarioId, p.CodigoBarras }).IsUnique();

        entity.HasOne(p => p.Inventario)
            .WithMany()
            .HasForeignKey(p => p.InventarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
