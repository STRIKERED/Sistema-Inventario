using Inventario.Core.Entities;
using Inventario.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CotizacionesController : ControllerBase
{
    private readonly ICotizacionRepository _cotizacionRepository;
    private readonly ICotizacionPdfService _pdfService;

    public CotizacionesController(ICotizacionRepository cotizacionRepository, ICotizacionPdfService pdfService)
    {
        _cotizacionRepository = cotizacionRepository;
        _pdfService = pdfService;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Cotizacion>> ObtenerPorId(int id)
    {
        var cotizacion = await _cotizacionRepository.ObtenerPorIdAsync(id);
        if (cotizacion is null)
        {
            return NotFound();
        }

        return Ok(cotizacion);
    }

    [HttpGet("vigentes/{sucursalId:int}")]
    public async Task<ActionResult<IEnumerable<Cotizacion>>> ObtenerVigentes(int sucursalId)
    {
        var cotizaciones = await _cotizacionRepository.ObtenerVigentesAsync(sucursalId);
        return Ok(cotizaciones);
    }

    // Nota: al crear, dejar Detalles[].Producto en null y solo fijar ProductoId,
    // para que EF no intente insertar un producto duplicado junto con la cotización.
    [HttpPost]
    public async Task<ActionResult<Cotizacion>> Crear(Cotizacion cotizacion)
    {
        var creada = await _cotizacionRepository.CrearAsync(cotizacion);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.Id }, creada);
    }

    [HttpGet("{id:int}/pdf")]
    public async Task<IActionResult> DescargarPdf(int id)
    {
        var cotizacion = await _cotizacionRepository.ObtenerPorIdAsync(id);
        if (cotizacion is null)
        {
            return NotFound();
        }

        var pdf = _pdfService.GenerarPdf(cotizacion);
        return File(pdf, "application/pdf", $"cotizacion-{cotizacion.Folio}.pdf");
    }
}
