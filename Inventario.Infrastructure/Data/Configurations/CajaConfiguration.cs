using Inventario.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Infrastructure.Data.Configurations;

public class CajaConfiguration : IEntityTypeConfiguration<Caja>
{
    public void Configure(EntityTypeBuilder<Caja> entity)
    {
        entity.Property(c => c.Nombre).HasMaxLength(100).IsRequired();
        entity.HasOne(c => c.Inventario)
            .WithMany()
            .HasForeignKey(c => c.InventarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
