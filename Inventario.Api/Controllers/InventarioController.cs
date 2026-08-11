using Inventario.Core.Entities;
using Inventario.Core.Enums;
using Inventario.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventarioController : ControllerBase
{
    private readonly IInventarioService _inventarioService;
    private readonly IStockPorSucursalRepository _stockRepository;
    private readonly IMovimientoInventarioRepository _movimientoRepository;

    public InventarioController(
        IInventarioService inventarioService,
        IStockPorSucursalRepository stockRepository,
        IMovimientoInventarioRepository movimientoRepository)
    {
        _inventarioService = inventarioService;
        _stockRepository = stockRepository;
        _movimientoRepository = movimientoRepository;
    }

    [HttpGet("stock/{productoId:int}/{sucursalId:int}")]
    public async Task<ActionResult<int>> ObtenerStock(int productoId, int sucursalId)
    {
        var stock = await _inventarioService.ObtenerStockAsync(productoId, sucursalId);
        return Ok(stock);
    }

    [HttpGet("stock/sucursal/{sucursalId:int}")]
    public async Task<ActionResult<IEnumerable<StockPorSucursal>>> ObtenerStockPorSucursal(int sucursalId)
    {
        var stock = await _stockRepository.ObtenerPorSucursalAsync(sucursalId);
        return Ok(stock);
    }

    [HttpGet("stock/producto/{productoId:int}")]
    public async Task<ActionResult<IEnumerable<StockPorSucursal>>> ObtenerStockPorProducto(int productoId)
    {
        var stock = await _stockRepository.ObtenerPorProductoAsync(productoId);
        return Ok(stock);
    }

    [HttpGet("movimientos/producto/{productoId:int}")]
    public async Task<ActionResult<IEnumerable<MovimientoInventario>>> ObtenerMovimientosPorProducto(int productoId)
    {
        var movimientos = await _movimientoRepository.ObtenerPorProductoAsync(productoId);
        return Ok(movimientos);
    }

    [HttpGet("movimientos/sucursal/{sucursalId:int}")]
    public async Task<ActionResult<IEnumerable<MovimientoInventario>>> ObtenerMovimientosPorSucursal(int sucursalId)
    {
        var movimientos = await _movimientoRepository.ObtenerPorSucursalAsync(sucursalId);
        return Ok(movimientos);
    }

    [HttpPost("movimientos")]
    public async Task<ActionResult<MovimientoInventario>> RegistrarMovimiento(RegistrarMovimientoRequest request)
    {
        try
        {
            var movimiento = await _inventarioService.RegistrarMovimientoAsync(
                request.ProductoId, request.SucursalId, request.Tipo, request.Cantidad, request.Motivo, request.UsuarioId);
            return Ok(movimiento);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost("transferencias")]
    public async Task<IActionResult> Transferir(TransferirStockRequest request)
    {
        try
        {
            await _inventarioService.TransferirStockAsync(
                request.ProductoId, request.SucursalOrigenId, request.SucursalDestinoId,
                request.Cantidad, request.UsuarioId, request.Motivo);
            return NoContent();
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    public record RegistrarMovimientoRequest(
        int ProductoId, int SucursalId, TipoMovimientoInventario Tipo, int Cantidad, string? Motivo, int? UsuarioId);

    public record TransferirStockRequest(
        int ProductoId, int SucursalOrigenId, int SucursalDestinoId, int Cantidad, int? UsuarioId, string? Motivo);
}
