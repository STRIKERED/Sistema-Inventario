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
    bool Activo,
    int InventarioId,
    string? InventarioNombre,
    int CantidadDisponible,
    int StockMinimo);

public record CrearProductoRequest(
    [Required, StringLength(50)] string Sku,
    [Required, StringLength(50)] string CodigoBarras,
    [Required, StringLength(200)] string Nombre,
    string? Categoria,
    string? Unidad,
    [Range(0, double.MaxValue)] decimal PrecioCosto,
    [Range(0, double.MaxValue)] decimal PrecioVenta,
    [Range(1, int.MaxValue)] int InventarioId,
    [Range(0, int.MaxValue)] int CantidadDisponible,
    [Range(0, int.MaxValue)] int StockMinimo);

public record ActualizarProductoRequest(
    [Required, StringLength(50)] string Sku,
    [Required, StringLength(50)] string CodigoBarras,
    [Required, StringLength(200)] string Nombre,
    string? Categoria,
    string? Unidad,
    [Range(0, double.MaxValue)] decimal PrecioCosto,
    [Range(0, double.MaxValue)] decimal PrecioVenta,
    bool Activo,
    [Range(1, int.MaxValue)] int InventarioId,
    [Range(0, int.MaxValue)] int CantidadDisponible,
    [Range(0, int.MaxValue)] int StockMinimo);
