using Inventario.Core.Entities;
using Inventario.Core.Enums;
using Inventario.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioRepository _usuarioRepository;

    public UsuariosController(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UsuarioResponse>>> ObtenerTodos()
    {
        var usuarios = await _usuarioRepository.ObtenerTodosAsync();
        return Ok(usuarios.Select(UsuarioResponse.DesdeEntidad));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UsuarioResponse>> ObtenerPorId(int id)
    {
        var usuario = await _usuarioRepository.ObtenerPorIdAsync(id);
        if (usuario is null)
        {
            return NotFound();
        }

        return Ok(UsuarioResponse.DesdeEntidad(usuario));
    }

    [HttpPost]
    public async Task<ActionResult<UsuarioResponse>> Crear(Usuario usuario)
    {
        // TODO: cuando se implemente autenticación (JWT, ver README), hashear la contraseña aquí
        // en vez de recibir PasswordHash tal cual desde el cliente.
        await _usuarioRepository.AgregarAsync(usuario);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = usuario.Id }, UsuarioResponse.DesdeEntidad(usuario));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, Usuario usuario)
    {
        if (id != usuario.Id)
        {
            return BadRequest("El id de la ruta no coincide con el id del usuario.");
        }

        var existente = await _usuarioRepository.ObtenerPorIdAsync(id);
        if (existente is null)
        {
            return NotFound();
        }

        await _usuarioRepository.ActualizarAsync(usuario);
        return NoContent();
    }

    // Excluye PasswordHash de las respuestas: nunca debe exponerse vía API, ni siquiera hasheado.
    public record UsuarioResponse(int Id, string NombreUsuario, string? NombreCompleto, RolUsuario Rol, bool Activo, int? SucursalId)
    {
        public static UsuarioResponse DesdeEntidad(Usuario usuario) =>
            new(usuario.Id, usuario.NombreUsuario, usuario.NombreCompleto, usuario.Rol, usuario.Activo, usuario.SucursalId);
    }
}
