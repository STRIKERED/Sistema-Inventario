using Inventario.Core.Entities;
using Inventario.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using InventarioEntity = Inventario.Core.Entities.Inventario;

namespace Inventario.Infrastructure.Data;

/// <summary>
/// Siembra los datos mínimos para que una instalación nueva (base SQLite recién creada) sea
/// utilizable. NO crea un usuario Administrador por defecto a propósito: eso ya lo resuelve
/// <c>AuthController.RegistroInicial</c>, que deja al operador elegir usuario/contraseña desde
/// la pantalla de login del Desktop en vez de enviar una contraseña hardcodeada en el código.
/// Sí siembra una Sucursal y un Inventario inicial: Administrador tiene acceso implícito a todos
/// los Inventarios activos, pero si no existe ninguno el login no tendría nada que ofrecer para
/// operar (ni forma de crear uno desde la UI sin antes tener uno).
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var sucursalRepository = services.GetRequiredService<ISucursalRepository>();
        var inventarioRepository = services.GetRequiredService<IInventarioRepository>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DbSeeder));

        var sucursales = await sucursalRepository.ObtenerTodasAsync();
        if (sucursales.Any())
        {
            return;
        }

        var sucursal = new Sucursal { Nombre = "Sucursal Principal" };
        await sucursalRepository.AgregarAsync(sucursal);

        await inventarioRepository.AgregarAsync(new InventarioEntity
        {
            Nombre = "Inventario Principal",
            SucursalId = sucursal.Id,
            Activo = true
        });

        logger.LogInformation(
            "Base de datos nueva: se creó la sucursal inicial 'Sucursal Principal' y el inventario 'Inventario Principal'.");
    }
}
