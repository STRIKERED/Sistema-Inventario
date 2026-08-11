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
    private readonly IPasswordHasher _passwordHasher;

    public UsuariosController(IUsuarioRepository usuarioRepository, IPasswordHasher passwordHasher)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
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
    public async Task<ActionResult<UsuarioResponse>> Crear(CrearUsuarioRequest request)
    {
        var usuario = new Usuario
        {
            NombreUsuario = request.NombreUsuario,
            PasswordHash = _passwordHasher.Hashear(request.Password),
            NombreCompleto = request.NombreCompleto,
            Rol = request.Rol,
            SucursalId = request.SucursalId,
            Activo = true
        };

        await _usuarioRepository.AgregarAsync(usuario);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = usuario.Id }, UsuarioResponse.DesdeEntidad(usuario));
    }

    // No incluye la contraseña: para cambiarla haría falta un endpoint dedicado que la vuelva a hashear.
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, ActualizarUsuarioRequest request)
    {
        var existente = await _usuarioRepository.ObtenerPorIdAsync(id);
        if (existente is null)
        {
            return NotFound();
        }

        existente.NombreUsuario = request.NombreUsuario;
        existente.NombreCompleto = request.NombreCompleto;
        existente.Rol = request.Rol;
        existente.Activo = request.Activo;
        existente.SucursalId = request.SucursalId;

        await _usuarioRepository.ActualizarAsync(existente);
        return NoContent();
    }

    public record CrearUsuarioRequest(
        string NombreUsuario, string Password, string? NombreCompleto, RolUsuario Rol, int? SucursalId);

    public record ActualizarUsuarioRequest(
        string NombreUsuario, string? NombreCompleto, RolUsuario Rol, bool Activo, int? SucursalId);

    // Excluye PasswordHash de las respuestas: nunca debe exponerse vía API, ni siquiera hasheado.
    public record UsuarioResponse(int Id, string NombreUsuario, string? NombreCompleto, RolUsuario Rol, bool Activo, int? SucursalId)
    {
        public static UsuarioResponse DesdeEntidad(Usuario usuario) =>
            new(usuario.Id, usuario.NombreUsuario, usuario.NombreCompleto, usuario.Rol, usuario.Activo, usuario.SucursalId);
    }
}
