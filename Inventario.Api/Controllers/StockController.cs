using Inventario.Core.Dtos;
using Inventario.Core.Interfaces;
using Inventario.Core.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Api.Controllers;

// Antes "InventarioController": se renombra para no convivir de forma confusa con
// InventariosController (CRUD de la entidad Inventario). Este controller es sobre stock/movimientos
// de un Producto puntual, que ya implica su Inventario.
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockController : ControllerBase
{
    private readonly IStockService _stockService;
    private readonly IMovimientoInventarioRepository _movimientoRepository;

    public StockController(IStockService stockService, IMovimientoInventarioRepository movimientoRepository)
    {
        _stockService = stockService;
        _movimientoRepository = movimientoRepository;
    }

    [HttpGet("{productoId:int}")]
    public async Task<ActionResult<int>> ObtenerStock(int productoId)
    {
        var stock = await _stockService.ObtenerStockAsync(productoId);
        return Ok(stock);
    }

    [HttpGet("movimientos/producto/{productoId:int}")]
    public async Task<ActionResult<IEnumerable<MovimientoInventarioDto>>> ObtenerMovimientosPorProducto(int productoId)
    {
        var movimientos = await _movimientoRepository.ObtenerPorProductoAsync(productoId);
        return Ok(movimientos.ToDto());
    }

    // Ajustes de inventario (altas manuales, mermas, conteos) quedan reservados a Administrador/Gerente:
    // un Cajero o Vendedor no debería poder mover stock fuera del flujo normal de venta.
    [HttpPost("movimientos")]
    [Authorize(Roles = "Administrador,Gerente")]
    public async Task<ActionResult<MovimientoInventarioDto>> RegistrarMovimiento(RegistrarMovimientoRequest request)
    {
        var movimiento = await _stockService.RegistrarMovimientoAsync(
            request.ProductoId, request.Tipo, request.Cantidad, request.Motivo, request.UsuarioId);
        return Ok(movimiento.ToDto());
    }
}
