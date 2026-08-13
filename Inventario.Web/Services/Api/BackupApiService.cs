using Inventario.Web.Services.Http;

namespace Inventario.Web.Services.Api;

/// <summary>Exporta/importa el archivo SQLite completo de esta sucursal. Solo Administrador
/// (la Api lo restringe; ver BackupController).</summary>
public interface IBackupApiService
{
    Task<byte[]> ExportarAsync(CancellationToken ct = default);

    /// <summary>Si el esquema del respaldo no coincide con el de esta instalación, la Api responde
    /// 409 (<see cref="ApiException.StatusCode"/>) a menos que <paramref name="forzar"/> sea true.</summary>
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
