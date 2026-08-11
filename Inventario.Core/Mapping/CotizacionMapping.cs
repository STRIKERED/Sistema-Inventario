using Inventario.Core.Dtos;
using Inventario.Core.Entities;

namespace Inventario.Core.Mapping;

public static class CotizacionMapping
{
    public static DetalleCotizacionDto ToDto(this DetalleCotizacion detalle) =>
        new(detalle.Id, detalle.ProductoId, detalle.Producto?.Nombre, detalle.Cantidad, detalle.PrecioUnitario,
            detalle.Cantidad * detalle.PrecioUnitario);

    public static CotizacionDto ToDto(this Cotizacion cotizacion) =>
        new(cotizacion.Id, cotizacion.Folio, cotizacion.ClienteNombre, cotizacion.ClienteContacto,
            cotizacion.FechaCreacion, cotizacion.FechaVigencia, cotizacion.Estado, cotizacion.Subtotal,
            cotizacion.Descuento, cotizacion.Impuestos, cotizacion.Total, cotizacion.SucursalId,
            cotizacion.Sucursal?.Nombre, cotizacion.UsuarioId, cotizacion.Usuario?.NombreUsuario,
            cotizacion.Detalles.Select(d => d.ToDto()).ToList());

    public static IEnumerable<CotizacionDto> ToDto(this IEnumerable<Cotizacion> cotizaciones) =>
        cotizaciones.Select(c => c.ToDto());
}
