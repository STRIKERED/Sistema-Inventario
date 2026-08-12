using Inventario.Core.Dtos;
using Inventario.Core.Entities;
using Inventario.Core.Enums;
using Inventario.Core.Interfaces;
using Inventario.Core.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CotizacionesController : ControllerBase
{
    private readonly ICotizacionRepository _cotizacionRepository;
    private readonly IVentaRepository _ventaRepository;
    private readonly IProductoRepository _productoRepository;
    private readonly IStockService _stockService;
    private readonly IFolioService _folioService;
    private readonly ICalculadoraTotalesService _calculadoraTotales;
    private readonly ICotizacionPdfService _pdfService;
    private readonly ILogger<CotizacionesController> _logger;

    public CotizacionesController(
        ICotizacionRepository cotizacionRepository,
        IVentaRepository ventaRepository,
        IProductoRepository productoRepository,
        IStockService stockService,
        IFolioService folioService,
        ICalculadoraTotalesService calculadoraTotales,
        ICotizacionPdfService pdfService,
        ILogger<CotizacionesController> logger)
    {
        _cotizacionRepository = cotizacionRepository;
        _ventaRepository = ventaRepository;
        _productoRepository = productoRepository;
        _stockService = stockService;
        _folioService = folioService;
        _calculadoraTotales = calculadoraTotales;
        _pdfService = pdfService;
        _logger = logger;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CotizacionDto>> ObtenerPorId(int id)
    {
        var cotizacion = await _cotizacionRepository.ObtenerPorIdAsync(id);
        if (cotizacion is null)
        {
            return NotFound();
        }

        return Ok(cotizacion.ToDto());
    }

    [HttpGet("vigentes/{inventarioId:int}")]
    public async Task<ActionResult<IEnumerable<CotizacionDto>>> ObtenerVigentes(int inventarioId)
    {
        var cotizaciones = await _cotizacionRepository.ObtenerVigentesAsync(inventarioId);
        return Ok(cotizaciones.ToDto());
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Gerente,Cajero,Vendedor")]
    public async Task<ActionResult<CotizacionDto>> Crear(CrearCotizacionRequest request)
    {
        // El precio de cada línea se toma de Producto.PrecioVenta al momento de cotizar (no se recibe
        // del cliente), igual que en Ventas: así la cotización refleja el precio vigente y no uno manipulado.
        var detalles = new List<DetalleCotizacion>();
        foreach (var linea in request.Detalles)
        {
            var producto = await _productoRepository.ObtenerPorIdAsync(linea.ProductoId);
            if (producto is null)
            {
                return BadRequest($"El producto {linea.ProductoId} no existe.");
            }

            detalles.Add(new DetalleCotizacion
            {
                ProductoId = linea.ProductoId,
                Cantidad = linea.Cantidad,
                PrecioUnitario = producto.PrecioVenta
            });
        }

        var subtotal = detalles.Sum(d => d.Cantidad * d.PrecioUnitario);
        var (impuestos, total) = _calculadoraTotales.Calcular(subtotal, request.Descuento);

        var cotizacion = new Cotizacion
        {
            Folio = string.Empty, // se fija después del insert, a partir del Id ya asignado (ver IFolioService)
            ClienteNombre = request.ClienteNombre,
            ClienteContacto = request.ClienteContacto,
            FechaCreacion = DateTime.UtcNow,
            FechaVigencia = request.FechaVigencia,
            Estado = EstadoCotizacion.Vigente,
            Subtotal = subtotal,
            Descuento = request.Descuento,
            Impuestos = impuestos,
            Total = total,
            InventarioId = request.InventarioId,
            UsuarioId = request.UsuarioId,
            Detalles = detalles
        };

        var creada = await _cotizacionRepository.CrearAsync(cotizacion);

        var folio = _folioService.GenerarFolioCotizacion(creada.Id);
        await _cotizacionRepository.ActualizarFolioAsync(creada.Id, folio);

        var completa = await _cotizacionRepository.ObtenerPorIdAsync(creada.Id);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.Id }, completa!.ToDto());
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
    [Authorize(Roles = "Administrador,Gerente,Cajero,Vendedor")]
    public async Task<ActionResult<VentaDto>> ConvertirAVenta(int id, ConvertirAVentaRequest request)
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

        // Validar disponibilidad de TODAS las líneas antes de registrar nada.
        foreach (var detalle in cotizacion.Detalles)
        {
            var disponible = await _stockService.ValidarStockDisponibleAsync(detalle.ProductoId, detalle.Cantidad);
            if (!disponible)
            {
                return Conflict(
                    $"Stock insuficiente para el producto '{detalle.Producto?.Nombre ?? detalle.ProductoId.ToString()}'.");
            }
        }

        var venta = new Venta
        {
            Folio = string.Empty,
            Fecha = DateTime.UtcNow,
            MetodoPago = request.MetodoPago,
            Subtotal = cotizacion.Subtotal,
            Descuento = cotizacion.Descuento,
            Impuestos = cotizacion.Impuestos,
            Total = cotizacion.Total,
            InventarioId = cotizacion.InventarioId,
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

        var folio = _folioService.GenerarFolioVenta(creada.Id);
        await _ventaRepository.ActualizarFolioAsync(creada.Id, folio);

        foreach (var detalle in creada.Detalles)
        {
            try
            {
                await _stockService.RegistrarMovimientoAsync(
                    detalle.ProductoId, TipoMovimientoInventario.Salida, detalle.Cantidad,
                    $"Venta {folio} (desde cotización {cotizacion.Folio})", creada.UsuarioId);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex,
                    "No se pudo descontar stock del producto {ProductoId} al convertir la cotización {CotizacionId} en la venta {VentaId}: {Mensaje}",
                    detalle.ProductoId, id, creada.Id, ex.Message);
            }
        }

        cotizacion.Estado = EstadoCotizacion.Convertida;
        await _cotizacionRepository.ActualizarAsync(cotizacion);

        var completa = await _ventaRepository.ObtenerPorIdAsync(creada.Id);
        return Created($"/api/ventas/{creada.Id}", completa!.ToDto());
    }
}
