using Inventario.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Infrastructure.Data.Configurations;

public class SucursalConfiguration : IEntityTypeConfiguration<Sucursal>
{
    public void Configure(EntityTypeBuilder<Sucursal> entity)
    {
        entity.Property(s => s.Nombre).HasMaxLength(300).IsRequired();
        entity.Property(s => s.Direccion).HasMaxLength(600);
    }
}
