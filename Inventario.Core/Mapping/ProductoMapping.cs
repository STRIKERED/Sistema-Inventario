using Inventario.Core.Dtos;
using Inventario.Core.Entities;

namespace Inventario.Core.Mapping;

public static class ProductoMapping
{
    public static ProductoDto ToDto(this Producto producto) =>
        new(producto.Id, producto.Sku, producto.CodigoBarras, producto.Nombre, producto.Categoria,
            producto.Unidad, producto.PrecioCosto, producto.PrecioVenta, producto.Activo);

    public static IEnumerable<ProductoDto> ToDto(this IEnumerable<Producto> productos) =>
        productos.Select(p => p.ToDto());

    public static Producto ToEntity(this CrearProductoRequest request) =>
        new()
        {
            Sku = request.Sku,
            CodigoBarras = request.CodigoBarras,
            Nombre = request.Nombre,
            Categoria = request.Categoria,
            Unidad = request.Unidad,
            PrecioCosto = request.PrecioCosto,
            PrecioVenta = request.PrecioVenta,
            Activo = true
        };

    public static void AplicarA(this ActualizarProductoRequest request, Producto producto)
    {
        producto.Sku = request.Sku;
        producto.CodigoBarras = request.CodigoBarras;
        producto.Nombre = request.Nombre;
        producto.Categoria = request.Categoria;
        producto.Unidad = request.Unidad;
        producto.PrecioCosto = request.PrecioCosto;
        producto.PrecioVenta = request.PrecioVenta;
        producto.Activo = request.Activo;
    }
}
