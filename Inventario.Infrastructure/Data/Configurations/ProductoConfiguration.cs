using Inventario.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Infrastructure.Data.Configurations;

public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> builder)
    {
        builder.ToTable("Productos");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Sku).HasMaxLength(50).IsRequired();
        builder.HasIndex(p => p.Sku).IsUnique();
        builder.Property(p => p.CodigoBarras).HasMaxLength(50).IsRequired();
        builder.HasIndex(p => p.CodigoBarras).IsUnique();
        builder.Property(p => p.Nombre).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Categoria).HasMaxLength(100);
        builder.Property(p => p.Unidad).HasMaxLength(20);
        builder.Property(p => p.PrecioCosto).HasColumnType("decimal(18,2)");
        builder.Property(p => p.PrecioVenta).HasColumnType("decimal(18,2)");
    }
}
