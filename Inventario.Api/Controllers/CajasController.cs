using Inventario.Core.Dtos;
using Inventario.Core.Interfaces;
using Inventario.Core.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CajasController : ControllerBase
{
    private readonly ICajaRepository _cajaRepository;

    public CajasController(ICajaRepository cajaRepository)
    {
        _cajaRepository = cajaRepository;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CajaDto>> ObtenerPorId(int id)
    {
        var caja = await _cajaRepository.ObtenerPorIdAsync(id);
        if (caja is null)
        {
            return NotFound();
        }

        return Ok(caja.ToDto());
    }

    [HttpGet("inventario/{inventarioId:int}")]
    public async Task<ActionResult<IEnumerable<CajaDto>>> ObtenerPorInventario(int inventarioId)
    {
        var cajas = await _cajaRepository.ObtenerPorInventarioAsync(inventarioId);
        return Ok(cajas.ToDto());
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Gerente")]
    public async Task<ActionResult<CajaDto>> Crear(CajaRequest request)
    {
        var caja = request.ToEntity();
        await _cajaRepository.AgregarAsync(caja);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = caja.Id }, caja.ToDto());
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador,Gerente")]
    public async Task<IActionResult> Actualizar(int id, CajaRequest request)
    {
        var existente = await _cajaRepository.ObtenerPorIdAsync(id);
        if (existente is null)
        {
            return NotFound();
        }

        request.AplicarA(existente);
        await _cajaRepository.ActualizarAsync(existente);
        return NoContent();
    }
}
