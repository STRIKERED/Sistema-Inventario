using Inventario.Core.Entities;
using Inventario.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly IProductoRepository _productoRepository;

    public ProductosController(IProductoRepository productoRepository)
    {
        _productoRepository = productoRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Producto>>> ObtenerTodos()
    {
        var productos = await _productoRepository.ObtenerTodosAsync();
        return Ok(productos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Producto>> ObtenerPorId(int id)
    {
        var producto = await _productoRepository.ObtenerPorIdAsync(id);
        if (producto is null)
        {
            return NotFound();
        }

        return Ok(producto);
    }

    [HttpGet("codigo-barras/{codigoBarras}")]
    public async Task<ActionResult<Producto>> ObtenerPorCodigoBarras(string codigoBarras)
    {
        var producto = await _productoRepository.ObtenerPorCodigoBarrasAsync(codigoBarras);
        if (producto is null)
        {
            return NotFound();
        }

        return Ok(producto);
    }

    [HttpPost]
    public async Task<ActionResult<Producto>> Crear(Producto producto)
    {
        await _productoRepository.AgregarAsync(producto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = producto.Id }, producto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, Producto producto)
    {
        if (id != producto.Id)
        {
            return BadRequest("El id de la ruta no coincide con el id del producto.");
        }

        var existente = await _productoRepository.ObtenerPorIdAsync(id);
        if (existente is null)
        {
            return NotFound();
        }

        await _productoRepository.ActualizarAsync(producto);
        return NoContent();
    }
}
