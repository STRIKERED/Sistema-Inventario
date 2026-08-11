namespace Inventario.Core.Dtos;

public record StockPorSucursalDto(
    int Id,
    int ProductoId,
    string? ProductoNombre,
    int SucursalId,
    string? SucursalNombre,
    int Cantidad);
