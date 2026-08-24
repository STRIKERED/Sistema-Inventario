using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Inventario.Core.Configuracion;
using Inventario.Desktop.Services.Backup;
using Inventario.Desktop.Services.Http;
using Inventario.Desktop.Services.Sesion;
using Microsoft.Maui.Storage;

namespace Inventario.Desktop.ViewModels;

/// <summary>
/// Exportar/importar el archivo SQLite completo de esta sucursal, para migrar entre PCs.
/// Exportar lee inventario.db directo (IBackupService.ExportarRespaldoAsync); importar siempre pasa
/// por la Api (solo ese proceso puede liberar su propio pool de conexiones antes de sobrescribir el
/// archivo con seguridad). Solo visible para Administrador (AppShell.xaml.cs controla el FlyoutItem).
/// </summary>
public partial class RespaldoViewModel : BaseViewModel
{
    private const string ClaveUltimoRespaldoLocal = "respaldo.ultimaExportacionLocal";

    private readonly IBackupService _backupService;

    public RespaldoViewModel(IBackupService backupService, ISessionService sessionService)
        : base(sessionService)
    {
        _backupService = backupService;

        if (Preferences.Default.ContainsKey(ClaveUltimoRespaldoLocal))
        {
            UltimoRespaldoLocal = Preferences.Default.Get(ClaveUltimoRespaldoLocal, DateTime.MinValue);
        }
    }

    [ObservableProperty]
    private DateTime? ultimoRespaldoLocal;

    partial void OnUltimoRespaldoLocalChanged(DateTime? value) => OnPropertyChanged(nameof(UltimoRespaldoLocalTexto));

    public string UltimoRespaldoLocalTexto => UltimoRespaldoLocal is { } fecha
        ? $"Último respaldo exportado desde este equipo: {fecha:dd/MM/yyyy HH:mm}"
        : "Todavía no se ha exportado ningún respaldo desde este equipo.";

    [RelayCommand]
    private async Task ExportarAsync()
    {
        await EjecutarAsync(async () =>
        {
            var carpetaBackups = Path.Combine(AppPaths.DataDirectory, "Backups");
            var ruta = await _backupService.ExportarRespaldoAsync(carpetaBackups);

            UltimoRespaldoLocal = DateTime.Now;
            Preferences.Default.Set(ClaveUltimoRespaldoLocal, UltimoRespaldoLocal.Value);

            if (Shell.Current is not null)
            {
                await Shell.Current.DisplayAlertAsync("Respaldo exportado", $"Se guardó en:\n{ruta}", "Aceptar");
            }
        });
    }

    [RelayCommand]
    private async Task ImportarAsync()
    {
        if (Shell.Current is null)
        {
            return;
        }

        var archivo = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Selecciona un respaldo (.zip exportado desde aquí, o un .db)",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, new[] { ".zip", ".db" } }
            })
        });

        if (archivo is null)
        {
            return;
        }

        var mensaje = $"Esto reemplaza TODOS los datos locales de esta sucursal por el contenido de '{archivo.FileName}'. " +
                      "Esta acción no se puede deshacer.";

        // Mejor esfuerzo: si es un .zip con manifest.json (el formato que exportan tanto esta pantalla
        // como BackupController), se muestra de qué sucursal/fecha es antes de confirmar. Un .db suelto
        // (sin zip) no tiene manifest — se sigue permitiendo importar, solo sin esta vista previa.
        try
        {
            var info = await _backupService.InspeccionarRespaldoAsync(archivo.FullPath);
            mensaje += $"\n\nSucursal del respaldo: {info.NombreSucursal ?? "desconocida"}" +
                       $"\nExportado: {(info.FechaExportacionUtc == DateTime.MinValue ? "desconocido" : info.FechaExportacionUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm"))}";
        }
        catch
        {
            // Sin vista previa disponible (no es un .zip válido con manifest) — no es motivo para
            // bloquear la importación, la Api hace su propia validación al recibir el archivo.
        }

        var confirmar = await Shell.Current.DisplayAlertAsync(
            "Importar respaldo", mensaje + "\n\n¿Continuar?", "Importar", "Cancelar");

        if (!confirmar)
        {
            return;
        }

        await EjecutarAsync(() => ImportarConReintentoAsync(archivo.FullPath, forzar: false));
    }

    // Separado de ImportarAsync para poder reintentar con forzar=true tras el 409 de "esquema
    // distinto" sin duplicar el manejo de IsBusy/errores de EjecutarAsync.
    private async Task ImportarConReintentoAsync(string rutaArchivo, bool forzar)
    {
        try
        {
            await _backupService.ImportarRespaldoAsync(rutaArchivo, forzar);
        }
        catch (ApiException ex) when (ex.StatusCode == 409 && !forzar)
        {
            var forzarConfirmado = Shell.Current is not null && await Shell.Current.DisplayAlertAsync(
                "Versión de esquema distinta",
                ex.Message + "\n\n¿Forzar la importación de todas formas?",
                "Forzar", "Cancelar");

            if (!forzarConfirmado)
            {
                return;
            }

            await ImportarConReintentoAsync(rutaArchivo, forzar: true);
            return;
        }

        if (Shell.Current is not null)
        {
            await Shell.Current.DisplayAlertAsync(
                "Respaldo importado",
                "Los datos se reemplazaron correctamente. Reinicia el servicio de Inventario.Api " +
                "para asegurar que todos los procesos queden usando la base nueva.",
                "Aceptar");
        }
    }
}
