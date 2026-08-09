using Inventario.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Infrastructure.Data.Configurations;

public class StockPorSucursalConfiguration : IEntityTypeConfiguration<StockPorSucursal>
{
    public void Configure(EntityTypeBuilder<StockPorSucursal> builder)
    {
        builder.ToTable("StockPorSucursal");
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => new { s.ProductoId, s.SucursalId }).IsUnique();

        builder.HasOne(s => s.Producto)
            .WithMany(p => p.Stocks)
            .HasForeignKey(s => s.ProductoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Sucursal)
            .WithMany(su => su.Stocks)
            .HasForeignKey(s => s.SucursalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
