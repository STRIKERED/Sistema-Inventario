using System.ComponentModel.DataAnnotations;
using Inventario.Core.Enums;

namespace Inventario.Core.Dtos;

public record CorteDeCajaDto(
    int Id,
    decimal MontoInicial,
    decimal MontoFinalContado,
    decimal MontoFinalSistema,
    decimal Diferencia,
    EstadoCorteDeCaja Estado,
    DateTime FechaApertura,
    DateTime? FechaCierre,
    int CajaId,
    string? CajaNombre,
    int UsuarioId,
    string? UsuarioNombre);

public record AbrirCorteRequest(
    [property: Range(1, int.MaxValue)] int CajaId,
    [property: Range(1, int.MaxValue)] int UsuarioId,
    [property: Range(0, double.MaxValue)] decimal MontoInicial);

public record CerrarCorteRequest(
    [property: Range(0, double.MaxValue)] decimal MontoFinalContado);
