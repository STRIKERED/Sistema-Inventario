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
    int? UsuarioId,
    string? UsuarioNombre);

// Sin [Range] en Cantidad: para Ajuste representa un delta con signo (puede ser negativo).
// La validación de positividad para Entrada/Salida ya la hace IStockService.
public record RegistrarMovimientoRequest(
    [Range(1, int.MaxValue)] int ProductoId,
    TipoMovimientoInventario Tipo,
    int Cantidad,
    string? Motivo,
    int? UsuarioId);
