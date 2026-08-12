using Inventario.Core.Dtos;
using InventarioEntity = Inventario.Core.Entities.Inventario;

namespace Inventario.Core.Mapping;

public static class InventarioMapping
{
    public static InventarioDto ToDto(this InventarioEntity inventario) =>
        new(inventario.Id, inventario.Nombre, inventario.Activo, inventario.SucursalId, inventario.Sucursal?.Nombre);

    public static IEnumerable<InventarioDto> ToDto(this IEnumerable<InventarioEntity> inventarios) =>
        inventarios.Select(i => i.ToDto());

    public static InventarioEntity ToEntity(this InventarioRequest request) =>
        new() { Nombre = request.Nombre, SucursalId = request.SucursalId, Activo = request.Activo };

    public static void AplicarA(this InventarioRequest request, InventarioEntity inventario)
    {
        inventario.Nombre = request.Nombre;
        inventario.SucursalId = request.SucursalId;
        inventario.Activo = request.Activo;
    }
}
