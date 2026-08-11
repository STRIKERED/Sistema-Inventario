using Inventario.Core.Dtos;
using Inventario.Core.Entities;

namespace Inventario.Core.Mapping;

public static class SucursalMapping
{
    public static SucursalDto ToDto(this Sucursal sucursal) =>
        new(sucursal.Id, sucursal.Nombre, sucursal.Direccion);

    public static IEnumerable<SucursalDto> ToDto(this IEnumerable<Sucursal> sucursales) =>
        sucursales.Select(s => s.ToDto());

    public static Sucursal ToEntity(this SucursalRequest request) =>
        new() { Nombre = request.Nombre, Direccion = request.Direccion };

    public static void AplicarA(this SucursalRequest request, Sucursal sucursal)
    {
        sucursal.Nombre = request.Nombre;
        sucursal.Direccion = request.Direccion;
    }
}
