using Inventario.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Infrastructure.Data.Configurations;

public class CotizacionConfiguration : IEntityTypeConfiguration<Cotizacion>
{
    public void Configure(EntityTypeBuilder<Cotizacion> builder)
    {
        builder.ToTable("Cotizaciones");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Folio).HasMaxLength(20).IsRequired();
        builder.HasIndex(c => c.Folio).IsUnique();
        builder.Property(c => c.ClienteNombre).HasMaxLength(200);
        builder.Property(c => c.ClienteContacto).HasMaxLength(150);
        builder.Property(c => c.Subtotal).HasColumnType("decimal(18,2)");
        builder.Property(c => c.Descuento).HasColumnType("decimal(18,2)");
        builder.Property(c => c.Impuestos).HasColumnType("decimal(18,2)");
        builder.Property(c => c.Total).HasColumnType("decimal(18,2)");

        builder.HasOne(c => c.Sucursal)
            .WithMany()
            .HasForeignKey(c => c.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Usuario)
            .WithMany()
            .HasForeignKey(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
