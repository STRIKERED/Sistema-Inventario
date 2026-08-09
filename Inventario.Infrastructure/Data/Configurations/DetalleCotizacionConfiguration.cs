using Inventario.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Infrastructure.Data.Configurations;

public class DetalleCotizacionConfiguration : IEntityTypeConfiguration<DetalleCotizacion>
{
    public void Configure(EntityTypeBuilder<DetalleCotizacion> builder)
    {
        builder.ToTable("DetallesCotizacion");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.PrecioUnitario).HasColumnType("decimal(18,2)");
        builder.Ignore(d => d.Subtotal);

        builder.HasOne(d => d.Cotizacion)
            .WithMany(c => c.Detalles)
            .HasForeignKey(d => d.CotizacionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Producto)
            .WithMany()
            .HasForeignKey(d => d.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
