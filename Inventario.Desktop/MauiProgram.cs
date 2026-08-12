using Inventario.Desktop.Configuracion;
using Inventario.Desktop.Services.Api;
using Inventario.Desktop.Services.Http;
using Inventario.Desktop.Services.Sesion;
using Inventario.Desktop.ViewModels;
using Inventario.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Inventario.Desktop;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        RegistrarServicios(builder.Services);
        RegistrarViewModelsYPaginas(builder.Services);

        return builder.Build();
    }

    private static void RegistrarServicios(IServiceCollection services)
    {
        services.AddSingleton<ISessionService, SessionService>();

        // El handler necesita una instancia nueva por HttpClient (cada AddHttpClient<> abajo arma el
        // suyo), así que se registra Transient en vez de Singleton.
        services.AddTransient<AuthHeaderHandler>();

        void ConfigurarCliente(HttpClient client) => client.BaseAddress = ApiConfig.ObtenerBaseAddress();

        services.AddHttpClient<IAuthApiService, AuthApiService>(ConfigurarCliente).AddHttpMessageHandler<AuthHeaderHandler>();
        services.AddHttpClient<IProductoApiService, ProductoApiService>(ConfigurarCliente).AddHttpMessageHandler<AuthHeaderHandler>();
        services.AddHttpClient<IVentaApiService, VentaApiService>(ConfigurarCliente).AddHttpMessageHandler<AuthHeaderHandler>();
        services.AddHttpClient<ICotizacionApiService, CotizacionApiService>(ConfigurarCliente).AddHttpMessageHandler<AuthHeaderHandler>();
        services.AddHttpClient<ICajaApiService, CajaApiService>(ConfigurarCliente).AddHttpMessageHandler<AuthHeaderHandler>();
        services.AddHttpClient<ISucursalApiService, SucursalApiService>(ConfigurarCliente).AddHttpMessageHandler<AuthHeaderHandler>();
        services.AddHttpClient<IUsuarioApiService, UsuarioApiService>(ConfigurarCliente).AddHttpMessageHandler<AuthHeaderHandler>();
        services.AddHttpClient<IBackupApiService, BackupApiService>(ConfigurarCliente).AddHttpMessageHandler<AuthHeaderHandler>();
    }

    private static void RegistrarViewModelsYPaginas(IServiceCollection services)
    {
        services.AddTransient<LoginViewModel>();
        services.AddTransient<VentaViewModel>();
        services.AddTransient<CajaViewModel>();
        services.AddTransient<CotizacionesViewModel>();
        services.AddTransient<UsuariosViewModel>();
        services.AddTransient<RespaldoViewModel>();

        services.AddTransient<LoginPage>();
        services.AddTransient<VentaPage>();
        services.AddTransient<CajaPage>();
        services.AddTransient<CotizacionesPage>();
        services.AddTransient<UsuariosPage>();
        services.AddTransient<RespaldoPage>();

        services.AddTransient<AppShell>();
    }
}
