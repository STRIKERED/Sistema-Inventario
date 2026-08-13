using Inventario.Core.Dtos;
using Inventario.Core.Interfaces;
using Inventario.Core.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConfiguracionesImpresionController : ControllerBase
{
    private static readonly int[] AnchosPermitidosMm = [58, 80];

    private readonly IConfiguracionImpresionRepository _configuracionRepository;

    public ConfiguracionesImpresionController(IConfiguracionImpresionRepository configuracionRepository)
    {
        _configuracionRepository = configuracionRepository;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ConfiguracionImpresionDto>> ObtenerPorId(int id)
    {
        var configuracion = await _configuracionRepository.ObtenerPorIdAsync(id);
        if (configuracion is null)
        {
            return NotFound();
        }

        return Ok(configuracion.ToDto());
    }

    [HttpGet("inventario/{inventarioId:int}")]
    public async Task<ActionResult<ConfiguracionImpresionDto>> ObtenerPorInventario(int inventarioId)
    {
        var configuracion = await _configuracionRepository.ObtenerPorInventarioAsync(inventarioId);
        if (configuracion is null)
        {
            return NotFound();
        }

        return Ok(configuracion.ToDto());
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Gerente")]
    public async Task<ActionResult<ConfiguracionImpresionDto>> Crear(ConfiguracionImpresionRequest request)
    {
        if (!AnchosPermitidosMm.Contains(request.AnchoTicketMm))
        {
            return BadRequest($"AnchoTicketMm debe ser uno de: {string.Join(", ", AnchosPermitidosMm)}.");
        }

        var existente = await _configuracionRepository.ObtenerPorInventarioAsync(request.InventarioId);
        if (existente is not null)
        {
            return Conflict($"El Inventario #{request.InventarioId} ya tiene una configuración de impresión (Id {existente.Id}). Usa PUT para modificarla.");
        }

        var configuracion = request.ToEntity();
        await _configuracionRepository.AgregarAsync(configuracion);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = configuracion.Id }, configuracion.ToDto());
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador,Gerente")]
    public async Task<IActionResult> Actualizar(int id, ConfiguracionImpresionRequest request)
    {
        if (!AnchosPermitidosMm.Contains(request.AnchoTicketMm))
        {
            return BadRequest($"AnchoTicketMm debe ser uno de: {string.Join(", ", AnchosPermitidosMm)}.");
        }

        var existente = await _configuracionRepository.ObtenerPorIdAsync(id);
        if (existente is null)
        {
            return NotFound();
        }

        if (existente.InventarioId != request.InventarioId)
        {
            var otra = await _configuracionRepository.ObtenerPorInventarioAsync(request.InventarioId);
            if (otra is not null)
            {
                return Conflict($"El Inventario #{request.InventarioId} ya tiene una configuración de impresión (Id {otra.Id}).");
            }
        }

        request.AplicarA(existente);
        await _configuracionRepository.ActualizarAsync(existente);
        return NoContent();
    }
}
