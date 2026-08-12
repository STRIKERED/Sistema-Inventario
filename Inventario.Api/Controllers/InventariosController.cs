using Inventario.Core.Dtos;
using Inventario.Core.Interfaces;
using Inventario.Core.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventariosController : ControllerBase
{
    private readonly IInventarioRepository _inventarioRepository;

    public InventariosController(IInventarioRepository inventarioRepository)
    {
        _inventarioRepository = inventarioRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventarioDto>>> ObtenerTodos()
    {
        var inventarios = await _inventarioRepository.ObtenerTodosAsync();
        return Ok(inventarios.ToDto());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<InventarioDto>> ObtenerPorId(int id)
    {
        var inventario = await _inventarioRepository.ObtenerPorIdAsync(id);
        if (inventario is null)
        {
            return NotFound();
        }

        return Ok(inventario.ToDto());
    }

    [HttpGet("sucursal/{sucursalId:int}")]
    public async Task<ActionResult<IEnumerable<InventarioDto>>> ObtenerPorSucursal(int sucursalId)
    {
        var inventarios = await _inventarioRepository.ObtenerPorSucursalAsync(sucursalId);
        return Ok(inventarios.ToDto());
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<InventarioDto>> Crear(InventarioRequest request)
    {
        var inventario = request.ToEntity();
        await _inventarioRepository.AgregarAsync(inventario);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = inventario.Id }, inventario.ToDto());
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Actualizar(int id, InventarioRequest request)
    {
        var existente = await _inventarioRepository.ObtenerPorIdAsync(id);
        if (existente is null)
        {
            return NotFound();
        }

        request.AplicarA(existente);
        await _inventarioRepository.ActualizarAsync(existente);
        return NoContent();
    }
}
