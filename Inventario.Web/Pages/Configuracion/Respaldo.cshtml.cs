using Inventario.Web.Services.Api;
using Inventario.Web.Services.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Configuracion
{
    /// <summary>Exportar/importar el archivo SQLite completo de esta sucursal, vía BackupController
    /// en la Api (esta página es un proxy autenticado: el navegador nunca habla con la Api
    /// directamente, todo pasa por esta Razor Pages app — ver AuthHeaderHandler). Solo Administrador.</summary>
    [Authorize(Roles = "Administrador")]
    [RequestSizeLimit(500_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 500_000_000)]
    public class RespaldoModel : PageModel
    {
        private readonly IBackupApiService _backupApiService;

        public RespaldoModel(IBackupApiService backupApiService)
        {
            _backupApiService = backupApiService;
        }

        [BindProperty]
        public IFormFile? Archivo { get; set; }

        [BindProperty]
        public bool Forzar { get; set; }

        public string? ErrorMensaje { get; private set; }
        public string? MensajeExito { get; private set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnGetExportarAsync()
        {
            try
            {
                var bytes = await _backupApiService.ExportarAsync();
                return File(bytes, "application/zip", $"inventario_{DateTime.Now:yyyy-MM-dd}.zip");
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                ErrorMensaje = ex.Message;
                return Page();
            }
        }

        public async Task<IActionResult> OnPostImportarAsync()
        {
            if (Archivo is null || Archivo.Length == 0)
            {
                ErrorMensaje = "Selecciona un archivo .zip (exportado desde aquí) o un .db.";
                return Page();
            }

            try
            {
                await using var stream = Archivo.OpenReadStream();
                await _backupApiService.ImportarAsync(Archivo.FileName, stream, Forzar);

                MensajeExito = "Respaldo importado correctamente. Reinicia Inventario.Api para asegurar " +
                               "que todas las conexiones queden usando la base nueva.";
            }
            catch (ApiException ex) when (ex.StatusCode == 409 && !Forzar)
            {
                ErrorMensaje = ex.Message + " Marca \"Forzar importación de todas formas\" y vuelve a intentar si estás seguro.";
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                ErrorMensaje = ex.Message;
            }

            return Page();
        }
    }
}
