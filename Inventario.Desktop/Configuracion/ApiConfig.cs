namespace Inventario.Desktop.Configuracion;

/// <summary>
/// Punto único donde vive la URL base de Inventario.Api. Ajusta el puerto/host si tu API corre en
/// otro lado (ver Inventario.Api/Properties/launchSettings.json para el puerto real de desarrollo).
///
/// HTTP en localhost:5025 a propósito (no HTTPS/7211): coincide con el perfil "http" de
/// launchSettings.json — el que toma `dotnet run` por defecto sin --launch-profile explícito — y con
/// el resto del sistema (Inventario.Web y el Windows Service de producción también usan 5025/http).
/// Usar HTTPS aquí obligaba a levantar la Api con el perfil "https" a mano cada vez, o la conexión se
/// rechazaba (nada escuchando en 7211).
/// </summary>
public static class ApiConfig
{
    public static Uri ObtenerBaseAddress()
    {
#if ANDROID
        // 10.0.2.2 es el alias que usa el emulador de Android para llegar al "localhost" de la máquina host.
        return new Uri("http://10.0.2.2:5025/");
#else
        // Windows, macOS (Mac Catalyst) e iOS (simulador) sí resuelven localhost directo.
        return new Uri("http://localhost:5025/");
#endif
    }
}
