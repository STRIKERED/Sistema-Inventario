namespace Inventario.Core.Configuracion;

/// <summary>
/// Rutas de datos locales de la aplicación. Cada sucursal tiene su propia base de datos SQLite
/// en la carpeta de datos de aplicación del usuario de Windows (%AppData%/InventarioApp).
/// Compartido entre Inventario.Api (dueño de la base) e Inventario.Desktop (para nombrar
/// respaldos exportados/importados con la misma convención).
/// </summary>
public static class AppPaths
{
    private const string CarpetaApp = "InventarioApp";
    private const string NombreArchivoBaseDeDatos = "inventario.db";

    public static string DataDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), CarpetaApp);

    public static string DatabaseFilePath => Path.Combine(DataDirectory, NombreArchivoBaseDeDatos);

    /// <summary>Crea la carpeta de datos si no existe. Debe llamarse antes de abrir la base de datos.</summary>
    public static void EnsureDataDirectoryExists() => Directory.CreateDirectory(DataDirectory);
}
