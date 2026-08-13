using Inventario.Core.Entities;

namespace Inventario.Core.Interfaces;

public interface IConfiguracionImpresionRepository
{
    Task<ConfiguracionImpresion?> ObtenerPorIdAsync(int id);
    Task<ConfiguracionImpresion?> ObtenerPorInventarioAsync(int inventarioId);
    Task AgregarAsync(ConfiguracionImpresion configuracion);
    Task ActualizarAsync(ConfiguracionImpresion configuracion);
}
