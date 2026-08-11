using Inventario.Core.Entities;
using Inventario.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CajasController : ControllerBase
{
    private readonly ICajaRepository _cajaRepository;

    public CajasController(ICajaRepository cajaRepository)
    {
        _cajaRepository = cajaRepository;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Caja>> ObtenerPorId(int id)
    {
        var caja = await _cajaRepository.ObtenerPorIdAsync(id);
        if (caja is null)
        {
            return NotFound();
        }

        return Ok(caja);
    }

    [HttpGet("sucursal/{sucursalId:int}")]
    public async Task<ActionResult<IEnumerable<Caja>>> ObtenerPorSucursal(int sucursalId)
    {
        var cajas = await _cajaRepository.ObtenerPorSucursalAsync(sucursalId);
        return Ok(cajas);
    }

    [HttpPost]
    public async Task<ActionResult<Caja>> Crear(Caja caja)
    {
        await _cajaRepository.AgregarAsync(caja);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = caja.Id }, caja);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, Caja caja)
    {
        if (id != caja.Id)
        {
            return BadRequest("El id de la ruta no coincide con el id de la caja.");
        }

        var existente = await _cajaRepository.ObtenerPorIdAsync(id);
        if (existente is null)
        {
            return NotFound();
        }

        await _cajaRepository.ActualizarAsync(caja);
        return NoContent();
    }
}
