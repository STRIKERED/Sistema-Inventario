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
public class VentasController : ControllerBase
{
    private readonly IVentaRepository _ventaRepository;
    private readonly IProductoRepository _productoRepository;
    private readonly IStockService _stockService;
    private readonly IFolioService _folioService;
    private readonly ICalculadoraTotalesService _calculadoraTotales;
    private readonly ITicketPrintService _ticketPrintService;
    private readonly ILogger<VentasController> _logger;

    public VentasController(
        IVentaRepository ventaRepository,
        IProductoRepository productoRepository,
        IStockService stockService,
        IFolioService folioService,
        ICalculadoraTotalesService calculadoraTotales,
        ITicketPrintService ticketPrintService,
        ILogger<VentasController> logger)
    {
        _ventaRepository = ventaRepository;
        _productoRepository = productoRepository;
        _stockService = stockService;
        _folioService = folioService;
        _calculadoraTotales = calculadoraTotales;
        _ticketPrintService = ticketPrintService;
        _logger = logger;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VentaDto>> ObtenerPorId(int id)
    {
        var venta = await _ventaRepository.ObtenerPorIdAsync(id);
        if (venta is null)
        {
            return NotFound();
        }

        return Ok(venta.ToDto());
    }

    [HttpGet("corte/{corteDeCajaId:int}")]
    public async Task<ActionResult<IEnumerable<VentaDto>>> ObtenerPorCorteDeCaja(int corteDeCajaId)
    {
        var ventas = await _ventaRepository.ObtenerPorCorteDeCajaAsync(corteDeCajaId);
        return Ok(ventas.ToDto());
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Gerente,Cajero,Vendedor")]
    public async Task<ActionResult<VentaDto>> Crear(CrearVentaRequest request)
    {
        // 1) Resolver precio real y validar stock de cada línea ANTES de tocar la base de datos.
        //    El precio se toma de Producto.PrecioVenta (no del request) para que el cliente no pueda
        //    manipularlo; solo el descuento por línea queda a discreción de quien vende.
        var detalles = new List<DetalleVenta>();
        foreach (var linea in request.Detalles)
        {
            var producto = await _productoRepository.ObtenerPorIdAsync(linea.ProductoId);
            if (producto is null)
            {
                return BadRequest($"El producto {linea.ProductoId} no existe.");
            }

            if (linea.DescuentoUnitario > producto.PrecioVenta)
            {
                return BadRequest($"El descuento unitario del producto '{producto.Nombre}' no puede superar su precio de venta.");
            }

            var disponible = await _stockService.ValidarStockDisponibleAsync(linea.ProductoId, linea.Cantidad);
            if (!disponible)
            {
                return Conflict($"Stock insuficiente para el producto '{producto.Nombre}'.");
            }

            detalles.Add(new DetalleVenta
            {
                ProductoId = linea.ProductoId,
                Cantidad = linea.Cantidad,
                PrecioUnitario = producto.PrecioVenta,
                DescuentoUnitario = linea.DescuentoUnitario
            });
        }

        // 2) Totales calculados en el servidor: nunca se confía en un Subtotal/Impuestos/Total enviado por el cliente.
        var subtotal = detalles.Sum(d => d.Cantidad * d.PrecioUnitario);
        var descuento = detalles.Sum(d => d.Cantidad * d.DescuentoUnitario);
        var (impuestos, total) = _calculadoraTotales.Calcular(subtotal, descuento);

        var venta = new Venta
        {
            Folio = string.Empty, // se fija después del insert, a partir del Id ya asignado (ver IFolioService)
            Fecha = DateTime.UtcNow,
            MetodoPago = request.MetodoPago,
            Subtotal = subtotal,
            Descuento = descuento,
            Impuestos = impuestos,
            Total = total,
            InventarioId = request.InventarioId,
            CorteDeCajaId = request.CorteDeCajaId,
            UsuarioId = request.UsuarioId,
            Detalles = detalles
        };

        var creada = await _ventaRepository.CrearAsync(venta);

        var folio = _folioService.GenerarFolioVenta(creada.Id);
        await _ventaRepository.ActualizarFolioAsync(creada.Id, folio);

        // 3) Descontar stock. El stock ya se validó arriba, pero puede haber cambiado entre la validación
        //    y este punto por una venta concurrente; si eso pasa, la venta ya quedó registrada (con el
        //    stock de esa línea sin descontar) y se deja constancia en el log. Una compensación/rollback
        //    completo requeriría una transacción distribuida, fuera del alcance de este cambio.
        foreach (var detalle in creada.Detalles)
        {
            try
            {
                await _stockService.RegistrarMovimientoAsync(
                    detalle.ProductoId, TipoMovimientoInventario.Salida, detalle.Cantidad,
                    $"Venta {folio}", creada.UsuarioId);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex,
                    "No se pudo descontar stock del producto {ProductoId} para la venta {VentaId} ({Folio}): {Mensaje}",
                    detalle.ProductoId, creada.Id, folio, ex.Message);
            }
        }

        var completa = await _ventaRepository.ObtenerPorIdAsync(creada.Id);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.Id }, completa!.ToDto());
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
