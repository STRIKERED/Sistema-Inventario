using System.ComponentModel.DataAnnotations;

namespace Inventario.Core.Dtos;

public record InventarioDto(int Id, string Nombre, bool Activo, int SucursalId, string? SucursalNombre);

public record InventarioRequest(
    [Required, StringLength(200)] string Nombre,
    [Range(1, int.MaxValue)] int SucursalId,
    bool Activo);
