using System.ComponentModel.DataAnnotations;
using Inventario.Core.Enums;

namespace Inventario.Core.Dtos;

public record MovimientoInventarioDto(
    int Id,
    TipoMovimientoInventario TipoMovimiento,
    int Cantidad,
    string? Motivo,
    DateTime Fecha,
    int ProductoId,
    string? ProductoNombre,
    int SucursalId,
    string? SucursalNombre,
    int? UsuarioId,
    string? UsuarioNombre);

// Sin [Range] en Cantidad: para Ajuste representa un delta con signo (puede ser negativo).
// La validación de positividad para Entrada/Salida ya la hace IInventarioService.
public record RegistrarMovimientoRequest(
    [property: Range(1, int.MaxValue)] int ProductoId,
    [property: Range(1, int.MaxValue)] int SucursalId,
    TipoMovimientoInventario Tipo,
    int Cantidad,
    string? Motivo,
    int? UsuarioId);

public record TransferirStockRequest(
    [property: Range(1, int.MaxValue)] int ProductoId,
    [property: Range(1, int.MaxValue)] int SucursalOrigenId,
    [property: Range(1, int.MaxValue)] int SucursalDestinoId,
    [property: Range(1, int.MaxValue)] int Cantidad,
    int? UsuarioId,
    string? Motivo);
