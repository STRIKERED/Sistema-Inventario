using System.ComponentModel.DataAnnotations;

namespace Inventario.Core.Dtos;

public record ProductoDto(
    int Id,
    string Sku,
    string CodigoBarras,
    string Nombre,
    string? Categoria,
    string? Unidad,
    decimal PrecioCosto,
    decimal PrecioVenta,
    bool Activo);

public record CrearProductoRequest(
    [property: Required, StringLength(50)] string Sku,
    [property: Required, StringLength(50)] string CodigoBarras,
    [property: Required, StringLength(200)] string Nombre,
    string? Categoria,
    string? Unidad,
    [property: Range(0, double.MaxValue)] decimal PrecioCosto,
    [property: Range(0, double.MaxValue)] decimal PrecioVenta);

public record ActualizarProductoRequest(
    [property: Required, StringLength(50)] string Sku,
    [property: Required, StringLength(50)] string CodigoBarras,
    [property: Required, StringLength(200)] string Nombre,
    string? Categoria,
    string? Unidad,
    [property: Range(0, double.MaxValue)] decimal PrecioCosto,
    [property: Range(0, double.MaxValue)] decimal PrecioVenta,
    bool Activo);
