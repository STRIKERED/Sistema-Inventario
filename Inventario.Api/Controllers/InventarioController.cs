using Inventario.Core.Dtos;
using Inventario.Core.Interfaces;
using Inventario.Core.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
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
    public async Task<ActionResult<IEnumerable<StockPorSucursalDto>>> ObtenerStockPorSucursal(int sucursalId)
    {
        var stock = await _stockRepository.ObtenerPorSucursalAsync(sucursalId);
        return Ok(stock.ToDto());
    }

    [HttpGet("stock/producto/{productoId:int}")]
    public async Task<ActionResult<IEnumerable<StockPorSucursalDto>>> ObtenerStockPorProducto(int productoId)
    {
        var stock = await _stockRepository.ObtenerPorProductoAsync(productoId);
        return Ok(stock.ToDto());
    }

    [HttpGet("movimientos/producto/{productoId:int}")]
    public async Task<ActionResult<IEnumerable<MovimientoInventarioDto>>> ObtenerMovimientosPorProducto(int productoId)
    {
        var movimientos = await _movimientoRepository.ObtenerPorProductoAsync(productoId);
        return Ok(movimientos.ToDto());
    }

    [HttpGet("movimientos/sucursal/{sucursalId:int}")]
    public async Task<ActionResult<IEnumerable<MovimientoInventarioDto>>> ObtenerMovimientosPorSucursal(int sucursalId)
    {
        var movimientos = await _movimientoRepository.ObtenerPorSucursalAsync(sucursalId);
        return Ok(movimientos.ToDto());
    }

    // Ajustes de inventario (altas manuales, mermas, conteos) quedan reservados a Administrador/Gerente:
    // un Cajero o Vendedor no debería poder mover stock fuera del flujo normal de venta.
    [HttpPost("movimientos")]
    [Authorize(Roles = "Administrador,Gerente")]
    public async Task<ActionResult<MovimientoInventarioDto>> RegistrarMovimiento(RegistrarMovimientoRequest request)
    {
        var movimiento = await _inventarioService.RegistrarMovimientoAsync(
            request.ProductoId, request.SucursalId, request.Tipo, request.Cantidad, request.Motivo, request.UsuarioId);
        return Ok(movimiento.ToDto());
    }

    [HttpPost("transferencias")]
    [Authorize(Roles = "Administrador,Gerente")]
    public async Task<IActionResult> Transferir(TransferirStockRequest request)
    {
        await _inventarioService.TransferirStockAsync(
            request.ProductoId, request.SucursalOrigenId, request.SucursalDestinoId,
            request.Cantidad, request.UsuarioId, request.Motivo);
        return NoContent();
    }
}
