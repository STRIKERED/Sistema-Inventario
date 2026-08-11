using Inventario.Core.Dtos;
using Inventario.Core.Entities;

namespace Inventario.Core.Mapping;

public static class MovimientoInventarioMapping
{
    public static MovimientoInventarioDto ToDto(this MovimientoInventario movimiento) =>
        new(movimiento.Id, movimiento.TipoMovimiento, movimiento.Cantidad, movimiento.Motivo, movimiento.Fecha,
            movimiento.ProductoId, movimiento.Producto?.Nombre, movimiento.SucursalId, movimiento.Sucursal?.Nombre,
            movimiento.UsuarioId, movimiento.Usuario?.NombreUsuario);

    public static IEnumerable<MovimientoInventarioDto> ToDto(this IEnumerable<MovimientoInventario> movimientos) =>
        movimientos.Select(m => m.ToDto());
}
