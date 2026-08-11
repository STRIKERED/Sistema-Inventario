using Inventario.Core.Dtos;
using Inventario.Core.Entities;
using Inventario.Core.Enums;
using Inventario.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
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

    // Público a propósito: el cliente lo consulta ANTES de poder autenticarse, para saber si debe
    // mostrar el login normal o el formulario de "crear el primer Administrador". Revela únicamente
    // si el sistema ya tiene algún usuario, no información sensible.
    [HttpGet("estado")]
    public async Task<ActionResult<EstadoSistemaResponse>> ObtenerEstado()
    {
        var hayUsuarios = await _usuarioRepository.ExisteAlgunoAsync();
        return Ok(new EstadoSistemaResponse(hayUsuarios));
    }

    // Sin [Authorize]: es el único punto de entrada posible cuando la base de datos no tiene ningún
    // usuario todavía (alta del primer Administrador). Se cierra solo: en cuanto exista un usuario,
    // este endpoint responde 409 para siempre y la única forma de crear cuentas vuelve a ser
    // UsuariosController (que sí exige un Administrador autenticado).
    // Nota: existe una ventana de carrera minúscula entre el ExisteAlgunoAsync() y el AgregarAsync()
    // si dos solicitudes llegan a la vez con la base vacía; aceptable para un alta única de arranque,
    // no se justifica una transacción/lock exclusivo para este caso.
    [HttpPost("registro-inicial")]
    public async Task<ActionResult<LoginResponse>> RegistroInicial(RegistrarUsuarioInicialRequest request)
    {
        if (await _usuarioRepository.ExisteAlgunoAsync())
        {
            return Conflict("Ya hay usuarios registrados. Pide a un Administrador que te dé de alta desde la pantalla de Usuarios.");
        }

        var usuario = new Usuario
        {
            NombreUsuario = request.NombreUsuario,
            PasswordHash = _passwordHasher.Hashear(request.Password),
            NombreCompleto = request.NombreCompleto,
            Rol = RolUsuario.Administrador,
            Activo = true
        };

        await _usuarioRepository.AgregarAsync(usuario);

        var token = _jwtTokenService.GenerarToken(usuario);

        return Ok(new LoginResponse(
            token,
            usuario.Id,
            usuario.NombreUsuario,
            usuario.NombreCompleto,
            usuario.Rol,
            usuario.SucursalId));
    }
}
