using Inventario.Core.Entities;
using Inventario.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SucursalesController : ControllerBase
{
    private readonly ISucursalRepository _sucursalRepository;

    public SucursalesController(ISucursalRepository sucursalRepository)
    {
        _sucursalRepository = sucursalRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Sucursal>>> ObtenerTodas()
    {
        var sucursales = await _sucursalRepository.ObtenerTodasAsync();
        return Ok(sucursales);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Sucursal>> ObtenerPorId(int id)
    {
        var sucursal = await _sucursalRepository.ObtenerPorIdAsync(id);
        if (sucursal is null)
        {
            return NotFound();
        }

        return Ok(sucursal);
    }

    [HttpPost]
    public async Task<ActionResult<Sucursal>> Crear(Sucursal sucursal)
    {
        await _sucursalRepository.AgregarAsync(sucursal);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = sucursal.Id }, sucursal);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, Sucursal sucursal)
    {
        if (id != sucursal.Id)
        {
            return BadRequest("El id de la ruta no coincide con el id de la sucursal.");
        }

        var existente = await _sucursalRepository.ObtenerPorIdAsync(id);
        if (existente is null)
        {
            return NotFound();
        }

        await _sucursalRepository.ActualizarAsync(sucursal);
        return NoContent();
    }
}
