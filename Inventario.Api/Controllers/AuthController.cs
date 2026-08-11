using Inventario.Core.Enums;
using Inventario.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(
        IUsuarioRepository usuarioRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var usuario = await _usuarioRepository.ObtenerPorNombreUsuarioAsync(request.NombreUsuario);

        if (usuario is null || !usuario.Activo || !_passwordHasher.Verificar(request.Password, usuario.PasswordHash))
        {
            // Mensaje genérico a propósito: no revelar si fue el usuario o la contraseña lo que falló.
            return Unauthorized("Usuario o contraseña incorrectos.");
        }

        var token = _jwtTokenService.GenerarToken(usuario);

        return Ok(new LoginResponse(
            token,
            usuario.Id,
            usuario.NombreUsuario,
            usuario.NombreCompleto,
            usuario.Rol,
            usuario.SucursalId));
    }

    public record LoginRequest(string NombreUsuario, string Password);

    public record LoginResponse(
        string Token,
        int UsuarioId,
        string NombreUsuario,
        string? NombreCompleto,
        RolUsuario Rol,
        int? SucursalId);
}
