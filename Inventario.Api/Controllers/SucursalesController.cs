using Inventario.Core.Dtos;
using Inventario.Core.Interfaces;
using Inventario.Core.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SucursalesController : ControllerBase
{
    private readonly ISucursalRepository _sucursalRepository;

    public SucursalesController(ISucursalRepository sucursalRepository)
    {
        _sucursalRepository = sucursalRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SucursalDto>>> ObtenerTodas()
    {
        var sucursales = await _sucursalRepository.ObtenerTodasAsync();
        return Ok(sucursales.ToDto());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SucursalDto>> ObtenerPorId(int id)
    {
        var sucursal = await _sucursalRepository.ObtenerPorIdAsync(id);
        if (sucursal is null)
        {
            return NotFound();
        }

        return Ok(sucursal.ToDto());
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<SucursalDto>> Crear(SucursalRequest request)
    {
        var sucursal = request.ToEntity();
        await _sucursalRepository.AgregarAsync(sucursal);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = sucursal.Id }, sucursal.ToDto());
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Actualizar(int id, SucursalRequest request)
    {
        var existente = await _sucursalRepository.ObtenerPorIdAsync(id);
        if (existente is null)
        {
            return NotFound();
        }

        request.AplicarA(existente);
        await _sucursalRepository.ActualizarAsync(existente);
        return NoContent();
    }
}
