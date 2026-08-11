using Inventario.Core.Entities;
using Inventario.Core.Enums;
using Inventario.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CotizacionesController : ControllerBase
{
    private readonly ICotizacionRepository _cotizacionRepository;
    private readonly IVentaRepository _ventaRepository;
    private readonly ICotizacionPdfService _pdfService;

    public CotizacionesController(
        ICotizacionRepository cotizacionRepository,
        IVentaRepository ventaRepository,
        ICotizacionPdfService pdfService)
    {
        _cotizacionRepository = cotizacionRepository;
        _ventaRepository = ventaRepository;
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

    [HttpPost("{id:int}/convertir-a-venta")]
    public async Task<ActionResult<Venta>> ConvertirAVenta(int id, ConvertirAVentaRequest request)
    {
        var cotizacion = await _cotizacionRepository.ObtenerPorIdAsync(id);
        if (cotizacion is null)
        {
            return NotFound();
        }

        if (cotizacion.Estado != EstadoCotizacion.Vigente)
        {
            return Conflict($"La cotización {id} no está vigente (estado actual: {cotizacion.Estado}).");
        }

        var venta = new Venta
        {
            Folio = $"V-{cotizacion.Folio}",
            Fecha = DateTime.UtcNow,
            MetodoPago = request.MetodoPago,
            Subtotal = cotizacion.Subtotal,
            Descuento = cotizacion.Descuento,
            Impuestos = cotizacion.Impuestos,
            Total = cotizacion.Total,
            SucursalId = cotizacion.SucursalId,
            CorteDeCajaId = request.CorteDeCajaId,
            UsuarioId = request.UsuarioId,
            Detalles = cotizacion.Detalles.Select(d => new DetalleVenta
            {
                ProductoId = d.ProductoId,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                DescuentoUnitario = 0
            }).ToList()
        };

        var creada = await _ventaRepository.CrearAsync(venta);

        cotizacion.Estado = EstadoCotizacion.Convertida;
        await _cotizacionRepository.ActualizarAsync(cotizacion);

        return Created($"/api/ventas/{creada.Id}", creada);
    }

    public record ConvertirAVentaRequest(int UsuarioId, int CorteDeCajaId, MetodoPago MetodoPago);
}
