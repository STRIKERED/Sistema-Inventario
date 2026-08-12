using Inventario.Core.Entities;
using Inventario.Core.Enums;

namespace Inventario.Core.Interfaces;

/// <summary>
/// Lógica de stock/movimientos de un Producto. Se llama "Stock" (no "Inventario") a propósito, para
/// no chocar con la entidad <see cref="Entities.Inventario"/> (el contenedor al que pertenece el
/// Producto) — antes de ese refactor este servicio se llamaba IInventarioService.
/// </summary>
public interface IStockService
{
    /// <summary>Cantidad actual en stock de un producto (0 si no existe).</summary>
    Task<int> ObtenerStockAsync(int productoId);

    /// <summary>Indica si hay suficiente stock disponible para cubrir la cantidad requerida.</summary>
    Task<bool> ValidarStockDisponibleAsync(int productoId, int cantidadRequerida);

    /// <summary>
    /// Registra un movimiento de inventario (Entrada, Salida o Ajuste) y actualiza el stock del producto.
    /// Para Entrada/Salida, <paramref name="cantidad"/> debe ser positiva.
    /// Para Ajuste, <paramref name="cantidad"/> es el delta con signo a aplicar al stock actual.
    /// Lanza <see cref="InvalidOperationException"/> si el movimiento dejaría el stock en negativo.
    /// </summary>
    Task<MovimientoInventario> RegistrarMovimientoAsync(
        int productoId,
        TipoMovimientoInventario tipo,
        int cantidad,
        string? motivo = null,
        int? usuarioId = null);
}
