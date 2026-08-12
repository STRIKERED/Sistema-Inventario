using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Inventario.Core.Entities;
using Inventario.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Inventario.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerarToken(Usuario usuario)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var key = jwtSection["Key"]
            ?? throw new InvalidOperationException("Falta configurar Jwt:Key en appsettings.json.");
        var expirationMinutes = int.TryParse(jwtSection["ExpirationMinutes"], out var minutos) ? minutos : 60;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.NombreUsuario),
            new(ClaimTypes.Role, usuario.Rol.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Sin claim de inventario/sucursal: un usuario puede tener acceso a varios Inventarios
        // (UsuarioInventario), así que ese dato ya no cabe como un único valor fijo en el token. El
        // Inventario con el que se opera lo elige el cliente tras el login (ver LoginResponse.Inventarios)
        // y viaja en cada request via el body/ruta, igual que ya pasaba con el resto de los recursos.

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credenciales = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credenciales);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
