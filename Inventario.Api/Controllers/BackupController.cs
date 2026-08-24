using System.IO.Compression;
using System.Text.Json;
using Inventario.Core.Configuracion;
using Inventario.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers;

/// <summary>
/// Exporta/importa el archivo SQLite completo de esta sucursal para trasladar datos entre
/// máquinas (p. ej. migrar de PC, o llevar un respaldo a otra sucursal). Solo Administrador:
/// importar reemplaza toda la base de datos local.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class BackupController : ControllerBase
{
    private const string NombreArchivoBaseDeDatos = "inventario.db";
    private const string NombreManifiesto = "manifest.json";

    private readonly InventarioDbContext _context;
    private readonly ILogger<BackupController> _logger;

    public BackupController(InventarioDbContext context, ILogger<BackupController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // NombreSucursal es opcional a propósito: un respaldo armado fuera de este endpoint (p. ej. el
    // export directo de Inventario.Desktop, que lee el .db local sin pasar por aquí) puede no traerlo.
    private record BackupManifest(string SchemaVersion, DateTime ExportadoUtc, string? NombreSucursal);

    [HttpGet("exportar")]
    public async Task<IActionResult> Exportar()
    {
        // Vuelca el WAL al archivo principal para que la copia quede consistente sin tener que
        // detener la Api ni bloquear a los demás clientes.
        await _context.Database.ExecuteSqlRawAsync("PRAGMA wal_checkpoint(TRUNCATE);");

        var nombreSucursal = await _context.Sucursales.Select(s => s.Nombre).FirstOrDefaultAsync();
        var manifest = new BackupManifest(await ObtenerVersionEsquemaAsync(), DateTime.UtcNow, nombreSucursal);

        using var memoria = new MemoryStream();
        using (var zip = new ZipArchive(memoria, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entradaDb = zip.CreateEntry(NombreArchivoBaseDeDatos, CompressionLevel.Optimal);
            // FileShare.ReadWrite: el archivo sigue abierto por la conexión SQLite de este mismo
            // proceso (pool de EF Core), así que un OpenRead exclusivo fallaría con IOException.
            await using (var origen = new FileStream(AppPaths.DatabaseFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            await using (var destino = entradaDb.Open())
            {
                await origen.CopyToAsync(destino);
            }

            var entradaManifest = zip.CreateEntry(NombreManifiesto, CompressionLevel.Optimal);
            await using (var destino = entradaManifest.Open())
            {
                await JsonSerializer.SerializeAsync(destino, manifest);
            }
        }

        return File(memoria.ToArray(), "application/zip", $"inventario_{DateTime.Now:yyyy-MM-dd}.zip");
    }

    [HttpPost("importar")]
    [RequestSizeLimit(500_000_000)]
    public async Task<IActionResult> Importar(IFormFile archivo, [FromQuery] bool forzar = false)
    {
        if (archivo is null || archivo.Length == 0)
        {
            return BadRequest("Debes subir un archivo .zip (exportado desde aquí) o un .db.");
        }

        var carpetaTemporal = Path.Combine(Path.GetTempPath(), $"inventario-import-{Guid.NewGuid():N}");
        Directory.CreateDirectory(carpetaTemporal);
        try
        {
            var rutaSubida = Path.Combine(carpetaTemporal, archivo.FileName);
            await using (var destino = System.IO.File.Create(rutaSubida))
            {
                await archivo.CopyToAsync(destino);
            }

            string rutaDbNueva;
            string? versionBackup = null;

            if (Path.GetExtension(archivo.FileName).Equals(".zip", StringComparison.OrdinalIgnoreCase))
            {
                var carpetaExtraida = Path.Combine(carpetaTemporal, "extraido");
                ZipFile.ExtractToDirectory(rutaSubida, carpetaExtraida);

                rutaDbNueva = Path.Combine(carpetaExtraida, NombreArchivoBaseDeDatos);
                if (!System.IO.File.Exists(rutaDbNueva))
                {
                    return BadRequest("El .zip no contiene un archivo inventario.db válido (¿fue exportado desde este sistema?).");
                }

                var rutaManifest = Path.Combine(carpetaExtraida, NombreManifiesto);
                if (System.IO.File.Exists(rutaManifest))
                {
                    var manifest = JsonSerializer.Deserialize<BackupManifest>(
                        await System.IO.File.ReadAllTextAsync(rutaManifest));
                    versionBackup = manifest?.SchemaVersion;
                }
            }
            else
            {
                rutaDbNueva = rutaSubida;
            }

            var versionActual = await ObtenerVersionEsquemaAsync();
            if (!forzar && versionBackup is not null && versionBackup != versionActual)
            {
                // Texto plano (igual que los demás controllers): ApiServiceBase.AsegurarExitoAsync lo
                // toma tal cual como ApiException.Message; el Desktop distingue el reintento por el 409.
                return Conflict(
                    $"El respaldo (esquema '{versionBackup}') no coincide con el de esta instalación ('{versionActual}'). " +
                    "Importarlo puede fallar o perder datos. Confirma para forzar la importación de todas formas.");
            }

            // Suelta cualquier conexión pooled hacia inventario.db para poder sobrescribir el archivo.
            SqliteConnection.ClearAllPools();

            var rutaDbActual = AppPaths.DatabaseFilePath;
            var rutaRespaldoSeguridad = $"{rutaDbActual}.bak-{DateTime.Now:yyyyMMddHHmmss}";
            if (System.IO.File.Exists(rutaDbActual))
            {
                System.IO.File.Copy(rutaDbActual, rutaRespaldoSeguridad, overwrite: true);
            }

            System.IO.File.Copy(rutaDbNueva, rutaDbActual, overwrite: true);

            _logger.LogWarning(
                "Base de datos reemplazada desde un respaldo importado. Copia de seguridad previa guardada en {Ruta}.",
                rutaRespaldoSeguridad);

            return Ok(new
            {
                mensaje = "Respaldo importado correctamente. Reinicia el servicio de Inventario.Api para asegurar " +
                          "que todas las conexiones queden usando la base nueva.",
                respaldoSeguridad = rutaRespaldoSeguridad
            });
        }
        finally
        {
            Directory.Delete(carpetaTemporal, recursive: true);
        }
    }

    /// <summary>
    /// Identificador de esquema usado para comparar respaldos entre instalaciones: la última
    /// migración aplicada a esta base de datos (el startup de Program.cs siempre migra al arrancar,
    /// así que equivale a la última migración compilada en esta versión de la app).
    /// </summary>
    private async Task<string> ObtenerVersionEsquemaAsync()
    {
        var aplicadas = await _context.Database.GetAppliedMigrationsAsync();
        return aplicadas.OrderBy(id => id, StringComparer.Ordinal).LastOrDefault() ?? "sin-migraciones";
    }
}
