namespace Inventario.Desktop.Services.Backup;

/// <summary>
/// Respaldo para migrar datos entre PCs: exportar lee el .db local directamente (sin pasar por la
/// Api — es solo lectura, no hay riesgo de chocar con la conexión que la Api tiene abierta sobre el
/// mismo archivo); importar SIEMPRE pasa por la Api (<see cref="Api.IBackupApiService"/>), porque solo
/// el proceso de la Api puede liberar su propio pool de conexiones SQLite antes de sobrescribir el
/// archivo de forma segura — Desktop nunca escribe inventario.db directamente.
/// </summary>
public interface IBackupService
{
    /// <summary>Arma un .zip con una copia de inventario.db + manifest.json en carpetaDestino.
    /// Devuelve la ruta del .zip generado.</summary>
    Task<string> ExportarRespaldoAsync(string carpetaDestino);

    /// <summary>Lee la metadata de un respaldo (propio o exportado desde BackupController en la Api)
    /// sin importarlo — para mostrarle al usuario de qué sucursal/fecha es antes de confirmar.</summary>
    Task<InfoRespaldo> InspeccionarRespaldoAsync(string rutaArchivoRespaldo);

    /// <summary>
    /// Si el esquema del respaldo no coincide con el de esta instalación, la Api rechaza con 409
    /// a menos que <paramref name="forzar"/> sea true (ver BackupController.Importar).
    /// </summary>
    Task<ResultadoImportacion> ImportarRespaldoAsync(string rutaArchivoRespaldo, bool forzar = false);
}

/// <summary>VersionEsquema es "desconocida" para respaldos exportados directo desde Desktop (no se
/// arriesga a declarar una versión que podría desincronizarse con las migraciones reales; esa
/// validación la sigue haciendo la Api al importar, con el dato bueno). Los respaldos exportados vía
/// BackupController.Exportar sí traen la versión real.</summary>
public record InfoRespaldo(string VersionEsquema, DateTime FechaExportacionUtc, string? NombreSucursal);

public record ResultadoImportacion(bool Exitoso, string Mensaje);
