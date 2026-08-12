using Inventario.Core.Dtos;
using Inventario.Core.Entities;

namespace Inventario.Core.Mapping;

public static class CajaMapping
{
    public static CajaDto ToDto(this Caja caja) =>
        new(caja.Id, caja.Nombre, caja.InventarioId, caja.Inventario?.Nombre);

    public static IEnumerable<CajaDto> ToDto(this IEnumerable<Caja> cajas) =>
        cajas.Select(c => c.ToDto());

    public static Caja ToEntity(this CajaRequest request) =>
        new() { Nombre = request.Nombre, InventarioId = request.InventarioId };

    public static void AplicarA(this CajaRequest request, Caja caja)
    {
        caja.Nombre = request.Nombre;
        caja.InventarioId = request.InventarioId;
    }
}
