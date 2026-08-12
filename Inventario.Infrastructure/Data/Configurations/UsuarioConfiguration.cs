using Inventario.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Infrastructure.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> entity)
    {
        entity.Property(u => u.NombreUsuario).HasMaxLength(100).IsRequired();
        entity.Property(u => u.NombreCompleto).HasMaxLength(300);
        entity.HasIndex(u => u.NombreUsuario).IsUnique();
    }
}
