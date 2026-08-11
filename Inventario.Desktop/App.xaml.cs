using Microsoft.Extensions.DependencyInjection;

namespace Inventario.Desktop;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    // MAUI resuelve App vía el contenedor de DI (builder.UseMauiApp<App>()), así que el constructor
    // puede pedir IServiceProvider para armar AppShell (y todo lo que cuelgue de ella) con sus
    // dependencias inyectadas, en vez de instanciarla con "new".
    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var shell = _serviceProvider.GetRequiredService<AppShell>();
        return new Window(shell);
    }
}
