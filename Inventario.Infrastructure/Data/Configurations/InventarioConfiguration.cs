using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InventarioEntity = Inventario.Core.Entities.Inventario;

namespace Inventario.Infrastructure.Data.Configurations;

// El alias InventarioEntity evita chocar con el namespace raíz "Inventario" (Inventario.Api,
// Inventario.Core, ...): el nombre de la entidad es literalmente igual al namespace de la solución.
public class InventarioConfiguration : IEntityTypeConfiguration<InventarioEntity>
{
    public void Configure(EntityTypeBuilder<InventarioEntity> entity)
    {
        entity.Property(i => i.Nombre).HasMaxLength(200).IsRequired();
        entity.Property(i => i.Activo).HasDefaultValue(true);

        entity.HasOne(i => i.Sucursal)
            .WithMany(s => s.Inventarios)
            .HasForeignKey(i => i.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
