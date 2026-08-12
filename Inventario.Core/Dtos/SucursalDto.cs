using System.ComponentModel.DataAnnotations;

namespace Inventario.Core.Dtos;

public record SucursalDto(int Id, string Nombre, string? Direccion);

public record SucursalRequest(
    [Required, StringLength(150)] string Nombre,
    string? Direccion);
