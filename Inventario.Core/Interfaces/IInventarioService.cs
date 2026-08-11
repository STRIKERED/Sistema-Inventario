using Inventario.Core.Entities;
using Inventario.Core.Enums;

namespace Inventario.Core.Interfaces;

public interface IInventarioService
{
    /// <summary>Cantidad actual en stock de un producto en una sucursal (0 si no hay registro).</summary>
    Task<int> ObtenerStockAsync(int productoId, int sucursalId);

    /// <summary>Indica si hay suficiente stock disponible para cubrir la cantidad requerida.</summary>
    Task<bool> ValidarStockDisponibleAsync(int productoId, int sucursalId, int cantidadRequerida);

    /// <summary>
    /// Registra un movimiento de inventario (Entrada, Salida o Ajuste) y actualiza el stock correspondiente.
    /// Para Entrada/Salida, <paramref name="cantidad"/> debe ser positiva.
    /// Para Ajuste, <paramref name="cantidad"/> es el delta con signo a aplicar al stock actual.
    /// Lanza <see cref="InvalidOperationException"/> si el movimiento dejaría el stock en negativo.
    /// </summary>
    Task<MovimientoInventario> RegistrarMovimientoAsync(
        int productoId,
        int sucursalId,
        TipoMovimientoInventario tipo,
        int cantidad,
        string? motivo = null,
        int? usuarioId = null);

    /// <summary>
    /// Transfiere stock de una sucursal a otra de forma atómica: descuenta en origen, agrega en destino
    /// y registra un <see cref="MovimientoInventario"/> de tipo Transferencia en cada sucursal.
    /// Lanza <see cref="InvalidOperationException"/> si la sucursal de origen no tiene stock suficiente.
    /// </summary>
    Task TransferirStockAsync(
        int productoId,
        int sucursalOrigenId,
        int sucursalDestinoId,
        int cantidad,
        int? usuarioId = null,
        string? motivo = null);
}
