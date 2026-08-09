using Inventario.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Infrastructure.Data.Configurations;

public class CajaConfiguration : IEntityTypeConfiguration<Caja>
{
    public void Configure(EntityTypeBuilder<Caja> builder)
    {
        builder.ToTable("Cajas");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Nombre).HasMaxLength(50).IsRequired();

        builder.HasOne(c => c.Sucursal)
            .WithMany(s => s.Cajas)
            .HasForeignKey(c => c.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
