using System.IO.Compression;
using System.Text.Json;
using Inventario.Core.Configuracion;
using Inventario.Desktop.Services.Api;

namespace Inventario.Desktop.Services.Backup;

public class BackupService : IBackupService
{
    private const string NombreArchivoBaseDeDatos = "inventario.db";
    private const string NombreManifiesto = "manifest.json";

    private readonly IBackupApiService _backupApiService;

    public BackupService(IBackupApiService backupApiService)
    {
        _backupApiService = backupApiService;
    }

    public async Task<string> ExportarRespaldoAsync(string carpetaDestino)
    {
        Directory.CreateDirectory(carpetaDestino);

        var rutaZip = Path.Combine(carpetaDestino, $"inventario_{DateTime.Now:yyyy-MM-dd_HHmmss}.zip");

        using (var zip = ZipFile.Open(rutaZip, ZipArchiveMode.Create))
        {
            var entradaDb = zip.CreateEntry(NombreArchivoBaseDeDatos, CompressionLevel.Optimal);
            // FileShare.ReadWrite: el archivo sigue abierto por el proceso de Inventario.Api (pool de
            // conexiones de EF Core); un OpenRead exclusivo fallaría con IOException. Export es de solo
            // lectura, así que compartir el handle con la Api es seguro — a diferencia de importar,
            // donde SÍ hace falta que la Api libere el archivo antes de sobrescribirlo (por eso
            // ImportarRespaldoAsync nunca toca inventario.db directo: siempre sube el .zip a la Api).
            await using (var origen = new FileStream(AppPaths.DatabaseFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            await using (var destino = entradaDb.Open())
            {
                await origen.CopyToAsync(destino);
            }

            var entradaManifest = zip.CreateEntry(NombreManifiesto, CompressionLevel.Optimal);
            await using (var destino = entradaManifest.Open())
            {
                // Sin SchemaVersion a propósito (ver InfoRespaldo): la Api es quien valida la
                // compatibilidad real al importar, con el dato bueno (sus migraciones aplicadas).
                await JsonSerializer.SerializeAsync(destino, new { ExportadoUtc = DateTime.UtcNow });
            }
        }

        return rutaZip;
    }

    public async Task<InfoRespaldo> InspeccionarRespaldoAsync(string rutaArchivoRespaldo)
    {
        using var zip = ZipFile.OpenRead(rutaArchivoRespaldo);
        var entrada = zip.GetEntry(NombreManifiesto)
            ?? throw new InvalidOperationException("El archivo no es un respaldo válido (falta manifest.json).");

        await using var stream = entrada.Open();
        using var documento = await JsonDocument.ParseAsync(stream);
        var raiz = documento.RootElement;

        var version = raiz.TryGetProperty("SchemaVersion", out var v) && v.GetString() is { Length: > 0 } s ? s : "desconocida";
        var fecha = raiz.TryGetProperty("ExportadoUtc", out var f) ? f.GetDateTime() : DateTime.MinValue;
        var sucursal = raiz.TryGetProperty("NombreSucursal", out var n) ? n.GetString() : null;

        return new InfoRespaldo(version, fecha, sucursal);
    }

    // Sin try/catch: un fallo (401, 409 por esquema distinto, etc.) se deja escapar como ApiException,
    // igual que cualquier otra llamada a la Api en este proyecto — así RespaldoViewModel puede seguir
    // usando su mismo catch (ex.StatusCode == 409) para ofrecer "forzar" sin cambios de estructura.
    // ResultadoImportacion solo representa el caso de éxito (el llamador no necesita revisar .Exitoso
    // si captura ApiException, pero queda como el tipo que describe el resultado positivo).
    public async Task<ResultadoImportacion> ImportarRespaldoAsync(string rutaArchivoRespaldo, bool forzar = false)
    {
        await using var contenido = File.OpenRead(rutaArchivoRespaldo);
        await _backupApiService.ImportarAsync(Path.GetFileName(rutaArchivoRespaldo), contenido, forzar);

        return new ResultadoImportacion(true,
            "Respaldo importado correctamente. Reinicia Inventario.Api para asegurar que todas las conexiones queden usando la base nueva.");
    }
}
