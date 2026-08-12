using CommunityToolkit.Mvvm.Input;
using Inventario.Core.Configuracion;
using Inventario.Desktop.Services.Api;
using Inventario.Desktop.Services.Http;
using Inventario.Desktop.Services.Sesion;
using Microsoft.Maui.Storage;

namespace Inventario.Desktop.ViewModels;

/// <summary>
/// Exportar/importar el archivo SQLite completo de esta sucursal (ver BackupController en la Api).
/// Solo visible para Administrador (AppShell.xaml.cs controla el FlyoutItem).
/// </summary>
public partial class RespaldoViewModel : BaseViewModel
{
    private readonly IBackupApiService _backupApiService;

    public RespaldoViewModel(IBackupApiService backupApiService, ISessionService sessionService)
        : base(sessionService)
    {
        _backupApiService = backupApiService;
    }

    [RelayCommand]
    private async Task ExportarAsync()
    {
        await EjecutarAsync(async () =>
        {
            var bytes = await _backupApiService.ExportarAsync();

            var carpetaBackups = Path.Combine(AppPaths.DataDirectory, "Backups");
            Directory.CreateDirectory(carpetaBackups);

            var ruta = Path.Combine(carpetaBackups, $"inventario_{DateTime.Now:yyyy-MM-dd_HHmmss}.zip");
            await File.WriteAllBytesAsync(ruta, bytes);

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

        var confirmar = await Shell.Current.DisplayAlertAsync(
            "Importar respaldo",
            $"Esto reemplaza TODOS los datos locales de esta sucursal por el contenido de '{archivo.FileName}'. " +
            "Esta acción no se puede deshacer. ¿Continuar?",
            "Importar", "Cancelar");

        if (!confirmar)
        {
            return;
        }

        await EjecutarAsync(() => ImportarConReintentoAsync(archivo, forzar: false));
    }

    // Separado de ImportarAsync para poder reintentar con forzar=true tras el 409 de "esquema
    // distinto" sin duplicar la lectura del archivo elegido ni el manejo de IsBusy/errores de EjecutarAsync.
    private async Task ImportarConReintentoAsync(FileResult archivo, bool forzar)
    {
        try
        {
            await using var stream = await archivo.OpenReadAsync();
            await _backupApiService.ImportarAsync(archivo.FileName, stream, forzar);
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

            await ImportarConReintentoAsync(archivo, forzar: true);
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
