using Inventario.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Infrastructure.Data.Configurations;

public class VentaConfiguration : IEntityTypeConfiguration<Venta>
{
    public void Configure(EntityTypeBuilder<Venta> entity)
    {
        entity.Property(v => v.Folio).HasMaxLength(40).IsRequired();
        entity.Property(v => v.Cancelada).HasDefaultValue(false);
        entity.HasIndex(v => v.Folio).IsUnique();
        entity.HasOne(v => v.Inventario)
            .WithMany()
            .HasForeignKey(v => v.InventarioId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(v => v.CorteDeCaja)
            .WithMany()
            .HasForeignKey(v => v.CorteDeCajaId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(v => v.Usuario)
            .WithMany()
            .HasForeignKey(v => v.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
