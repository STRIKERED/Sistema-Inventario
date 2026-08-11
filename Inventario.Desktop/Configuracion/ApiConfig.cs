namespace Inventario.Desktop.Configuracion;

/// <summary>
/// Punto único donde vive la URL base de Inventario.Api. Ajusta el puerto/host si tu API corre en
/// otro lado (ver Inventario.Api/Properties/launchSettings.json para el puerto real de desarrollo).
/// </summary>
public static class ApiConfig
{
    public static Uri ObtenerBaseAddress()
    {
#if ANDROID
        // 10.0.2.2 es el alias que usa el emulador de Android para llegar al "localhost" de la máquina host.
        return new Uri("https://10.0.2.2:7211/");
#else
        // Windows, macOS (Mac Catalyst) e iOS (simulador) sí resuelven localhost directo.
        return new Uri("https://localhost:7211/");
#endif
    }
}
