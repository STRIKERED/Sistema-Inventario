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
        services.AddDbContext<InventarioDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Repositorios
        services.AddScoped<IProductoRepository, ProductoRepository>();
        services.AddScoped<IVentaRepository, VentaRepository>();
        services.AddScoped<ICotizacionRepository, CotizacionRepository>();
        services.AddScoped<ISucursalRepository, SucursalRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<ICajaRepository, CajaRepository>();
        services.AddScoped<ICorteDeCajaRepository, CorteDeCajaRepository>();
        services.AddScoped<IStockPorSucursalRepository, StockPorSucursalRepository>();
        services.AddScoped<IMovimientoInventarioRepository, MovimientoInventarioRepository>();

        // Servicios de dominio
        services.AddScoped<IInventarioService, InventarioService>();
        services.AddScoped<ITicketPrintService, TicketPrintService>();
        services.AddScoped<ICotizacionPdfService, CotizacionPdfService>();

        return services;
    }
}
