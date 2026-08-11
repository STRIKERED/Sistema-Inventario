using Inventario.Core.Entities;

namespace Inventario.Core.Interfaces;

public interface IJwtTokenService
{
    /// <summary>Genera un JWT firmado con las claims del usuario (id, nombre, rol, sucursal).</summary>
    string GenerarToken(Usuario usuario);
}
