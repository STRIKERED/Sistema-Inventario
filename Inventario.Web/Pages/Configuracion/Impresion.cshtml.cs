using System.ComponentModel.DataAnnotations;
using Inventario.Core.Dtos;
using Inventario.Web.Services.Api;
using Inventario.Web.Services.Http;
using Inventario.Web.Services.Sesion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Configuracion
{
    /// <summary>Alta/edición de la ConfiguracionImpresionDto del Inventario activo — mismo propósito
    /// que Inventario.Desktop.ViewModels.ConfiguracionImpresionViewModel, adaptado a Razor Pages.
    /// Restringido a Administrador en esta pantalla (la Api también permite Gerente, pero el resto de
    /// "Configuración" en Inventario.Web es exclusivo de Administrador — se mantiene consistente).</summary>
    [Authorize(Roles = "Administrador")]
    public class ImpresionModel : PageModel
    {
        private readonly IConfiguracionImpresionApiService _configuracionApiService;
        private readonly ICurrentSessionAccessor _sesionActual;

        private int? _configuracionId;

        public ImpresionModel(IConfiguracionImpresionApiService configuracionApiService, ICurrentSessionAccessor sesionActual)
        {
            _configuracionApiService = configuracionApiService;
            _sesionActual = sesionActual;
        }

        public List<int> AnchosDisponiblesMm { get; } = [58, 80];

        [BindProperty, Required(ErrorMessage = "El nombre de la impresora es obligatorio."), StringLength(200)]
        public string NombreImpresora { get; set; } = string.Empty;

        [BindProperty]
        public int AnchoTicketMm { get; set; } = 80;

        [BindProperty, StringLength(200)]
        public string? EncabezadoTicket { get; set; }

        [BindProperty, StringLength(200)]
        public string? PiePaginaTicket { get; set; }

        [BindProperty, StringLength(500)]
        public string? LogoRutaPdf { get; set; }

        public string? ErrorMensaje { get; private set; }
        public string? MensajeExito { get; private set; }

        public async Task OnGetAsync() => await CargarAsync();

        public async Task<IActionResult> OnPostAsync()
        {
            // El PageModel se crea de cero en cada request: a diferencia de un ViewModel de Desktop
            // (que vive mientras dura la pantalla), aquí _configuracionId no sobrevive entre el GET y
            // este POST. Hay que volver a resolverlo antes de decidir Crear vs. Actualizar.
            var inventarioId = _sesionActual.InventarioOperativoId!.Value;

            try
            {
                _configuracionId = (await _configuracionApiService.ObtenerPorInventarioAsync(inventarioId))?.Id;
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                ErrorMensaje = ex.Message;
                return Page();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var request = new ConfiguracionImpresionRequest(
                inventarioId,
                NombreImpresora.Trim(),
                AnchoTicketMm,
                string.IsNullOrWhiteSpace(EncabezadoTicket) ? null : EncabezadoTicket.Trim(),
                string.IsNullOrWhiteSpace(PiePaginaTicket) ? null : PiePaginaTicket.Trim(),
                string.IsNullOrWhiteSpace(LogoRutaPdf) ? null : LogoRutaPdf.Trim());

            try
            {
                if (_configuracionId is null)
                {
                    var creada = await _configuracionApiService.CrearAsync(request);
                    _configuracionId = creada.Id;
                }
                else
                {
                    await _configuracionApiService.ActualizarAsync(_configuracionId.Value, request);
                }

                MensajeExito = "Configuración guardada. Se usará en el próximo ticket o cotización que se imprima.";
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                ErrorMensaje = ex.Message;
            }

            return Page();
        }

        private async Task CargarAsync()
        {
            var inventarioId = _sesionActual.InventarioOperativoId!.Value;

            try
            {
                var configuracion = await _configuracionApiService.ObtenerPorInventarioAsync(inventarioId);

                _configuracionId = configuracion?.Id;
                NombreImpresora = configuracion?.NombreImpresora ?? string.Empty;
                AnchoTicketMm = configuracion?.AnchoTicketMm ?? 80;
                EncabezadoTicket = configuracion?.EncabezadoTicket;
                PiePaginaTicket = configuracion?.PiePaginaTicket;
                LogoRutaPdf = configuracion?.LogoRutaPdf;
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                ErrorMensaje = ex.Message;
            }
        }
    }
}
