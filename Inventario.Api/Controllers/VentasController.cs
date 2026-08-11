using Inventario.Core.Entities;
using Inventario.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VentasController : ControllerBase
{
    private readonly IVentaRepository _ventaRepository;
    private readonly ITicketPrintService _ticketPrintService;

    public VentasController(IVentaRepository ventaRepository, ITicketPrintService ticketPrintService)
    {
        _ventaRepository = ventaRepository;
        _ticketPrintService = ticketPrintService;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Venta>> ObtenerPorId(int id)
    {
        var venta = await _ventaRepository.ObtenerPorIdAsync(id);
        if (venta is null)
        {
            return NotFound();
        }

        return Ok(venta);
    }

    [HttpGet("corte/{corteDeCajaId:int}")]
    public async Task<ActionResult<IEnumerable<Venta>>> ObtenerPorCorteDeCaja(int corteDeCajaId)
    {
        var ventas = await _ventaRepository.ObtenerPorCorteDeCajaAsync(corteDeCajaId);
        return Ok(ventas);
    }

    // Nota: al crear, dejar Detalles[].Producto en null y solo fijar ProductoId,
    // para que EF no intente insertar un producto duplicado junto con la venta.
    [HttpPost]
    public async Task<ActionResult<Venta>> Crear(Venta venta)
    {
        var creada = await _ventaRepository.CrearAsync(venta);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.Id }, creada);
    }

    [HttpGet("{id:int}/ticket")]
    public async Task<IActionResult> DescargarTicket(int id)
    {
        var venta = await _ventaRepository.ObtenerPorIdAsync(id);
        if (venta is null)
        {
            return NotFound();
        }

        var bytes = _ticketPrintService.GenerarTicketEscPos(venta);
        return File(bytes, "application/octet-stream", $"ticket-{venta.Folio}.bin");
    }

    [HttpPost("{id:int}/imprimir")]
    public async Task<IActionResult> ImprimirTicket(int id, [FromQuery] string impresora)
    {
        var venta = await _ventaRepository.ObtenerPorIdAsync(id);
        if (venta is null)
        {
            return NotFound();
        }

        try
        {
            await _ticketPrintService.ImprimirTicketAsync(venta, impresora);
            return NoContent();
        }
        catch (PlatformNotSupportedException ex)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(ex.Message, statusCode: StatusCodes.Status502BadGateway);
        }
    }
}
