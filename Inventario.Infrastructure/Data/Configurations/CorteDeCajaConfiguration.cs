using Inventario.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Infrastructure.Data.Configurations;

public class CorteDeCajaConfiguration : IEntityTypeConfiguration<CorteDeCaja>
{
    public void Configure(EntityTypeBuilder<CorteDeCaja> builder)
    {
        builder.ToTable("CortesDeCaja");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.MontoInicial).HasColumnType("decimal(18,2)");
        builder.Property(c => c.MontoFinalContado).HasColumnType("decimal(18,2)");
        builder.Property(c => c.MontoFinalSistema).HasColumnType("decimal(18,2)");
        builder.Property(c => c.Diferencia).HasColumnType("decimal(18,2)");

        builder.HasOne(c => c.Caja)
            .WithMany(ca => ca.Cortes)
            .HasForeignKey(c => c.CajaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Usuario)
            .WithMany()
            .HasForeignKey(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
