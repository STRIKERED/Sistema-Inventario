using Inventario.Core.Entities;
using Inventario.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Inventario.Infrastructure.Data;

/// <summary>
/// Siembra los datos mínimos para que una instalación nueva (base SQLite recién creada) sea
/// utilizable. NO crea un usuario Administrador por defecto a propósito: eso ya lo resuelve
/// <c>AuthController.RegistroInicial</c>, que deja al operador elegir usuario/contraseña desde
/// la pantalla de login del Desktop en vez de enviar una contraseña hardcodeada en el código.
/// Lo único que falta para que ese flujo funcione en un arranque limpio es que exista al menos
/// una Sucursal (el alta de usuario no crea ninguna, y el login la exige para operar).
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var sucursalRepository = services.GetRequiredService<ISucursalRepository>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DbSeeder));

        var sucursales = await sucursalRepository.ObtenerTodasAsync();
        if (sucursales.Any())
        {
            return;
        }

        await sucursalRepository.AgregarAsync(new Sucursal { Nombre = "Sucursal Principal" });
        logger.LogInformation("Base de datos nueva: se creó la sucursal inicial 'Sucursal Principal'.");
    }
}
