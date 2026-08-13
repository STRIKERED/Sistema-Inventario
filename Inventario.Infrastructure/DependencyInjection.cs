using Inventario.Core.Configuracion;
using Inventario.Core.Interfaces;
using Inventario.Infrastructure.Data;
using Inventario.Infrastructure.Repositories;
using Inventario.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Inventario.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // Sin cadena explícita en appsettings: cada sucursal usa su propio archivo SQLite
            // local en %AppData%/InventarioApp/inventario.db.
            AppPaths.EnsureDataDirectoryExists();
            connectionString = $"Data Source={AppPaths.DatabaseFilePath}";
        }

        services.AddDbContext<InventarioDbContext>(options => options.UseSqlite(connectionString));

        // Repositorios
        services.AddScoped<IProductoRepository, ProductoRepository>();
        services.AddScoped<IVentaRepository, VentaRepository>();
        services.AddScoped<ICotizacionRepository, CotizacionRepository>();
        services.AddScoped<ISucursalRepository, SucursalRepository>();
        services.AddScoped<IInventarioRepository, InventarioRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<ICajaRepository, CajaRepository>();
        services.AddScoped<ICorteDeCajaRepository, CorteDeCajaRepository>();
        services.AddScoped<IMovimientoInventarioRepository, MovimientoInventarioRepository>();
        services.AddScoped<IConfiguracionImpresionRepository, ConfiguracionImpresionRepository>();

        // Servicios de dominio
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<ITicketPrintService, TicketPrintService>();
        services.AddScoped<ICotizacionPdfService, CotizacionPdfService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IFolioService, FolioService>();
        services.AddSingleton<ICalculadoraTotalesService, CalculadoraTotalesService>();

        return services;
    }
}
