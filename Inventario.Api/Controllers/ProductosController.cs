using Inventario.Core.Dtos;
using Inventario.Core.Interfaces;
using Inventario.Core.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductosController : ControllerBase
{
    private readonly IProductoRepository _productoRepository;

    public ProductosController(IProductoRepository productoRepository)
    {
        _productoRepository = productoRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductoDto>>> ObtenerTodos()
    {
        var productos = await _productoRepository.ObtenerTodosAsync();
        return Ok(productos.ToDto());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductoDto>> ObtenerPorId(int id)
    {
        var producto = await _productoRepository.ObtenerPorIdAsync(id);
        if (producto is null)
        {
            return NotFound();
        }

        return Ok(producto.ToDto());
    }

    [HttpGet("codigo-barras/{codigoBarras}")]
    public async Task<ActionResult<ProductoDto>> ObtenerPorCodigoBarras(string codigoBarras)
    {
        var producto = await _productoRepository.ObtenerPorCodigoBarrasAsync(codigoBarras);
        if (producto is null)
        {
            return NotFound();
        }

        return Ok(producto.ToDto());
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Gerente")]
    public async Task<ActionResult<ProductoDto>> Crear(CrearProductoRequest request)
    {
        var producto = request.ToEntity();
        await _productoRepository.AgregarAsync(producto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = producto.Id }, producto.ToDto());
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador,Gerente")]
    public async Task<IActionResult> Actualizar(int id, ActualizarProductoRequest request)
    {
        var existente = await _productoRepository.ObtenerPorIdAsync(id);
        if (existente is null)
        {
            return NotFound();
        }

        request.AplicarA(existente);
        await _productoRepository.ActualizarAsync(existente);
        return NoContent();
    }
}
