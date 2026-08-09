using Inventario.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Infrastructure.Data.Configurations;

public class VentaConfiguration : IEntityTypeConfiguration<Venta>
{
    public void Configure(EntityTypeBuilder<Venta> builder)
    {
        builder.ToTable("Ventas");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Folio).HasMaxLength(20).IsRequired();
        builder.HasIndex(v => v.Folio).IsUnique();
        builder.Property(v => v.Subtotal).HasColumnType("decimal(18,2)");
        builder.Property(v => v.Descuento).HasColumnType("decimal(18,2)");
        builder.Property(v => v.Impuestos).HasColumnType("decimal(18,2)");
        builder.Property(v => v.Total).HasColumnType("decimal(18,2)");

        builder.HasOne(v => v.Sucursal)
            .WithMany()
            .HasForeignKey(v => v.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.CorteDeCaja)
            .WithMany(c => c.Ventas)
            .HasForeignKey(v => v.CorteDeCajaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Usuario)
            .WithMany()
            .HasForeignKey(v => v.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
