using Inventario.Desktop.Services.Http;

namespace Inventario.Desktop.Services.Api;

public interface IBackupApiService
{
    /// <summary>Descarga el .zip con el respaldo completo de la base de datos de esta sucursal.</summary>
    Task<byte[]> ExportarAsync(CancellationToken ct = default);

    /// <summary>
    /// Sube un respaldo (.zip exportado desde aquí, o un .db suelto) para reemplazar la base local.
    /// Si el esquema del respaldo no coincide con el de esta instalación, la Api responde 409
    /// (<see cref="ApiException.StatusCode"/>) a menos que <paramref name="forzar"/> sea true.
    /// </summary>
    Task ImportarAsync(string nombreArchivo, Stream contenido, bool forzar = false, CancellationToken ct = default);
}

public class BackupApiService : ApiServiceBase, IBackupApiService
{
    public BackupApiService(HttpClient httpClient) : base(httpClient)
    {
    }

    public Task<byte[]> ExportarAsync(CancellationToken ct = default) =>
        GetBytesAsync("api/backup/exportar", ct);

    public Task ImportarAsync(string nombreArchivo, Stream contenido, bool forzar = false, CancellationToken ct = default)
    {
        var url = forzar ? "api/backup/importar?forzar=true" : "api/backup/importar";
        return PostFileAsync(url, "archivo", nombreArchivo, contenido, ct);
    }
}
