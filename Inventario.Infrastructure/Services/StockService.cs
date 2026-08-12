using Inventario.Core.Entities;
using Inventario.Core.Enums;
using Inventario.Core.Interfaces;
using Inventario.Infrastructure.Data;

namespace Inventario.Infrastructure.Services;

public class StockService : IStockService
{
    private readonly InventarioDbContext _context;
    private readonly IProductoRepository _productoRepository;
    private readonly IMovimientoInventarioRepository _movimientoRepository;

    public StockService(
        InventarioDbContext context,
        IProductoRepository productoRepository,
        IMovimientoInventarioRepository movimientoRepository)
    {
        _context = context;
        _productoRepository = productoRepository;
        _movimientoRepository = movimientoRepository;
    }

    public async Task<int> ObtenerStockAsync(int productoId)
    {
        var producto = await _productoRepository.ObtenerPorIdAsync(productoId);
        return producto?.CantidadDisponible ?? 0;
    }

    public async Task<bool> ValidarStockDisponibleAsync(int productoId, int cantidadRequerida)
    {
        var disponible = await ObtenerStockAsync(productoId);
        return disponible >= cantidadRequerida;
    }

    public async Task<MovimientoInventario> RegistrarMovimientoAsync(
        int productoId,
        TipoMovimientoInventario tipo,
        int cantidad,
        string? motivo = null,
        int? usuarioId = null)
    {
        int delta = tipo switch
        {
            TipoMovimientoInventario.Entrada => RequerirPositivo(cantidad),
            TipoMovimientoInventario.Salida => -RequerirPositivo(cantidad),
            TipoMovimientoInventario.Ajuste => cantidad,
            _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, "Tipo de movimiento no soportado.")
        };

        await using var transaction = await _context.Database.BeginTransactionAsync();

        await AjustarStockAsync(productoId, delta);

        var movimiento = new MovimientoInventario
        {
            ProductoId = productoId,
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

    /// <summary>Aplica un delta (positivo o negativo) al stock de un producto.</summary>
    private async Task AjustarStockAsync(int productoId, int delta)
    {
        var producto = await _productoRepository.ObtenerPorIdAsync(productoId)
            ?? throw new InvalidOperationException($"No existe el producto {productoId}.");

        var nuevaCantidad = producto.CantidadDisponible + delta;
        if (nuevaCantidad < 0)
        {
            throw new InvalidOperationException(
                $"Stock insuficiente para el producto {productoId}. " +
                $"Disponible: {producto.CantidadDisponible}, solicitado: {-delta}.");
        }

        producto.CantidadDisponible = nuevaCantidad;
        await _productoRepository.ActualizarAsync(producto);
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
