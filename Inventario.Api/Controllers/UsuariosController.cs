using Inventario.Core.Dtos;
using Inventario.Core.Interfaces;
using Inventario.Core.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador,Gerente")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _passwordHasher;

    public UsuariosController(IUsuarioRepository usuarioRepository, IPasswordHasher passwordHasher)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UsuarioDto>>> ObtenerTodos()
    {
        var usuarios = await _usuarioRepository.ObtenerTodosAsync();
        return Ok(usuarios.ToDto());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UsuarioDto>> ObtenerPorId(int id)
    {
        var usuario = await _usuarioRepository.ObtenerPorIdAsync(id);
        if (usuario is null)
        {
            return NotFound();
        }

        return Ok(usuario.ToDto());
    }

    // Alta y baja de usuarios (con hash de contraseña) quedan reservadas a Administrador:
    // Gerente puede consultar la lista, pero no crear cuentas nuevas.
    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<UsuarioDto>> Crear(CrearUsuarioRequest request)
    {
        var usuario = request.ToEntity(_passwordHasher.Hashear(request.Password));
        await _usuarioRepository.AgregarAsync(usuario);
        await _usuarioRepository.SincronizarInventariosAsync(usuario.Id, request.InventarioIds);

        var completo = await _usuarioRepository.ObtenerPorIdAsync(usuario.Id);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = usuario.Id }, completo!.ToDto());
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Actualizar(int id, ActualizarUsuarioRequest request)
    {
        var existente = await _usuarioRepository.ObtenerPorIdAsync(id);
        if (existente is null)
        {
            return NotFound();
        }

        request.AplicarA(existente);
        await _usuarioRepository.ActualizarAsync(existente);
        await _usuarioRepository.SincronizarInventariosAsync(id, request.InventarioIds);
        return NoContent();
    }
}
