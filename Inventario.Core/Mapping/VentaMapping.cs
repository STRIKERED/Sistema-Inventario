using Inventario.Core.Dtos;
using Inventario.Core.Entities;

namespace Inventario.Core.Mapping;

public static class VentaMapping
{
    public static DetalleVentaDto ToDto(this DetalleVenta detalle) =>
        new(detalle.Id, detalle.ProductoId, detalle.Producto?.Nombre, detalle.Cantidad, detalle.PrecioUnitario,
            detalle.DescuentoUnitario, detalle.Cantidad * (detalle.PrecioUnitario - detalle.DescuentoUnitario));

    public static VentaDto ToDto(this Venta venta) =>
        new(venta.Id, venta.Folio, venta.Fecha, venta.MetodoPago, venta.Subtotal, venta.Descuento, venta.Impuestos,
            venta.Total, venta.InventarioId, venta.Inventario?.Nombre, venta.CorteDeCajaId, venta.UsuarioId,
            venta.Usuario?.NombreUsuario, venta.Cancelada, venta.Detalles.Select(d => d.ToDto()).ToList());

    public static IEnumerable<VentaDto> ToDto(this IEnumerable<Venta> ventas) =>
        ventas.Select(v => v.ToDto());
}
