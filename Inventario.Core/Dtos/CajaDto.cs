using System.ComponentModel.DataAnnotations;

namespace Inventario.Core.Dtos;

public record CajaDto(int Id, string Nombre, int SucursalId, string? SucursalNombre);

public record CajaRequest(
    [Required, StringLength(100)] string Nombre,
    [Range(1, int.MaxValue)] int SucursalId);
