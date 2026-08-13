using Inventario.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Infrastructure.Data.Configurations;

public class ConfiguracionImpresionConfiguration : IEntityTypeConfiguration<ConfiguracionImpresion>
{
    public void Configure(EntityTypeBuilder<ConfiguracionImpresion> entity)
    {
        entity.Property(c => c.NombreImpresora).HasMaxLength(200).IsRequired();
        entity.Property(c => c.EncabezadoTicket).HasMaxLength(200);
        entity.Property(c => c.PiePaginaTicket).HasMaxLength(200);
        entity.Property(c => c.LogoRutaPdf).HasMaxLength(500);

        // Un Inventario tiene, a lo más, una configuración de impresión.
        entity.HasIndex(c => c.InventarioId).IsUnique();

        entity.HasOne(c => c.Inventario)
            .WithMany()
            .HasForeignKey(c => c.InventarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
