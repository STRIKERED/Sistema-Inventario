using Inventario.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Infrastructure.Data.Configurations;

public class UsuarioInventarioConfiguration : IEntityTypeConfiguration<UsuarioInventario>
{
    public void Configure(EntityTypeBuilder<UsuarioInventario> entity)
    {
        entity.HasKey(ui => new { ui.UsuarioId, ui.InventarioId });

        // Cascade en ambos lados: es una tabla puente pura, sin datos propios que preservar.
        entity.HasOne(ui => ui.Usuario)
            .WithMany(u => u.UsuarioInventarios)
            .HasForeignKey(ui => ui.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(ui => ui.Inventario)
            .WithMany(i => i.UsuarioInventarios)
            .HasForeignKey(ui => ui.InventarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
