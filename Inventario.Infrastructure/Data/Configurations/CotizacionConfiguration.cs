using Inventario.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Infrastructure.Data.Configurations;

public class CotizacionConfiguration : IEntityTypeConfiguration<Cotizacion>
{
    public void Configure(EntityTypeBuilder<Cotizacion> entity)
    {
        entity.Property(c => c.Folio).HasMaxLength(40).IsRequired();
        entity.Property(c => c.ClienteNombre).HasMaxLength(400);
        entity.Property(c => c.ClienteContacto).HasMaxLength(300);
        entity.Property(c => c.FechaCreacion).HasColumnName("Fecha");
        entity.HasIndex(c => c.Folio).IsUnique();
        entity.HasOne(c => c.Inventario)
            .WithMany()
            .HasForeignKey(c => c.InventarioId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(c => c.Usuario)
            .WithMany()
            .HasForeignKey(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
