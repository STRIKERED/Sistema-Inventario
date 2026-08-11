using Inventario.Core.Entities;
using Inventario.Core.Enums;
using Inventario.Core.Interfaces;
using Inventario.Infrastructure.Data;

namespace Inventario.Infrastructure.Services;

public class InventarioService : IInventarioService
{
    private readonly InventarioDbContext _context;
    private readonly IStockPorSucursalRepository _stockRepository;
    private readonly IMovimientoInventarioRepository _movimientoRepository;

    public InventarioService(
        InventarioDbContext context,
        IStockPorSucursalRepository stockRepository,
        IMovimientoInventarioRepository movimientoRepository)
    {
        _context = context;
        _stockRepository = stockRepository;
        _movimientoRepository = movimientoRepository;
    }

    public async Task<int> ObtenerStockAsync(int productoId, int sucursalId)
    {
        var stock = await _stockRepository.ObtenerAsync(productoId, sucursalId);
        return stock?.Cantidad ?? 0;
    }

    public async Task<bool> ValidarStockDisponibleAsync(int productoId, int sucursalId, int cantidadRequerida)
    {
        var disponible = await ObtenerStockAsync(productoId, sucursalId);
        return disponible >= cantidadRequerida;
    }

    public async Task<MovimientoInventario> RegistrarMovimientoAsync(
        int productoId,
        int sucursalId,
        TipoMovimientoInventario tipo,
        int cantidad,
        string? motivo = null,
        int? usuarioId = null)
    {
        if (tipo == TipoMovimientoInventario.Transferencia)
        {
            throw new InvalidOperationException(
                "Los movimientos de tipo Transferencia deben registrarse con TransferirStockAsync.");
        }

        int delta = tipo switch
        {
            TipoMovimientoInventario.Entrada => RequerirPositivo(cantidad),
            TipoMovimientoInventario.Salida => -RequerirPositivo(cantidad),
            TipoMovimientoInventario.Ajuste => cantidad,
            _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, "Tipo de movimiento no soportado.")
        };

        await using var transaction = await _context.Database.BeginTransactionAsync();

        await AjustarStockAsync(productoId, sucursalId, delta);

        var movimiento = new MovimientoInventario
        {
            ProductoId = productoId,
            SucursalId = sucursalId,
            TipoMovimiento = tipo,
            Cantidad = cantidad,
            Motivo = motivo,
            UsuarioId = usuarioId,
            Fecha = DateTime.UtcNow
        };
        await _movimientoRepository.CrearAsync(movimiento);

        await transaction.CommitAsync();

        return movimiento;
    }

    public async Task TransferirStockAsync(
        int productoId,
        int sucursalOrigenId,
        int sucursalDestinoId,
        int cantidad,
        int? usuarioId = null,
        string? motivo = null)
    {
        RequerirPositivo(cantidad);

        if (sucursalOrigenId == sucursalDestinoId)
        {
            throw new InvalidOperationException("La sucursal de origen y destino no pueden ser la misma.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        await AjustarStockAsync(productoId, sucursalOrigenId, -cantidad);
        await AjustarStockAsync(productoId, sucursalDestinoId, cantidad);

        await _movimientoRepository.CrearAsync(new MovimientoInventario
        {
            ProductoId = productoId,
            SucursalId = sucursalOrigenId,
            TipoMovimiento = TipoMovimientoInventario.Transferencia,
            Cantidad = cantidad,
            Motivo = motivo ?? $"Transferencia saliente a sucursal {sucursalDestinoId}",
            UsuarioId = usuarioId,
            Fecha = DateTime.UtcNow
        });

        await _movimientoRepository.CrearAsync(new MovimientoInventario
        {
            ProductoId = productoId,
            SucursalId = sucursalDestinoId,
            TipoMovimiento = TipoMovimientoInventario.Transferencia,
            Cantidad = cantidad,
            Motivo = motivo ?? $"Transferencia entrante desde sucursal {sucursalOrigenId}",
            UsuarioId = usuarioId,
            Fecha = DateTime.UtcNow
        });

        await transaction.CommitAsync();
    }

    /// <summary>Aplica un delta (positivo o negativo) al stock de un producto en una sucursal, creando el registro si no existe.</summary>
    private async Task AjustarStockAsync(int productoId, int sucursalId, int delta)
    {
        var stock = await _stockRepository.ObtenerAsync(productoId, sucursalId);

        if (stock is null)
        {
            if (delta < 0)
            {
                throw new InvalidOperationException(
                    $"Stock insuficiente para el producto {productoId} en la sucursal {sucursalId}.");
            }

            await _stockRepository.AgregarAsync(new StockPorSucursal
            {
                ProductoId = productoId,
                SucursalId = sucursalId,
                Cantidad = delta
            });
            return;
        }

        var nuevaCantidad = stock.Cantidad + delta;
        if (nuevaCantidad < 0)
        {
            throw new InvalidOperationException(
                $"Stock insuficiente para el producto {productoId} en la sucursal {sucursalId}. " +
                $"Disponible: {stock.Cantidad}, solicitado: {-delta}.");
        }

        stock.Cantidad = nuevaCantidad;
        await _stockRepository.ActualizarAsync(stock);
    }

    private static int RequerirPositivo(int cantidad)
    {
        if (cantidad <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cantidad), cantidad, "La cantidad debe ser mayor a cero.");
        }

        return cantidad;
    }
}
