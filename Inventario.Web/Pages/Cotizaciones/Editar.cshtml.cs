using System.ComponentModel.DataAnnotations;
using Inventario.Core.Dtos;
using Inventario.Core.Enums;
using Inventario.Web.Services.Api;
using Inventario.Web.Services.Http;
using Inventario.Web.Services.Sesion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Cotizaciones
{
    /// <summary>Solo edita datos de "encabezado" (cliente, vigencia, estado) — las líneas y el
    /// importe no se tocan una vez creada la cotización, ver ActualizarCotizacionRequest.</summary>
    public class EditarModel : PageModel
    {
        private readonly ICotizacionApiService _cotizacionApiService;
        private readonly ICurrentSessionAccessor _sesionActual;

        public EditarModel(ICotizacionApiService cotizacionApiService, ICurrentSessionAccessor sesionActual)
        {
            _cotizacionApiService = cotizacionApiService;
            _sesionActual = sesionActual;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty, StringLength(200)]
        public string? ClienteNombre { get; set; }

        [BindProperty, StringLength(200)]
        public string? ClienteContacto { get; set; }

        [BindProperty, DataType(DataType.Date)]
        public DateTime? FechaVigencia { get; set; }

        [BindProperty]
        public EstadoCotizacion Estado { get; set; }

        public string? Folio { get; private set; }
        public string? ErrorMensaje { get; private set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var cotizacion = await _cotizacionApiService.ObtenerPorIdAsync(Id);
            if (cotizacion is null || cotizacion.InventarioId != _sesionActual.InventarioOperativoId)
            {
                return NotFound();
            }

            Folio = cotizacion.Folio;
            ClienteNombre = cotizacion.ClienteNombre;
            ClienteContacto = cotizacion.ClienteContacto;
            FechaVigencia = cotizacion.FechaVigencia;
            Estado = cotizacion.Estado;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var request = new ActualizarCotizacionRequest(
                string.IsNullOrWhiteSpace(ClienteNombre) ? null : ClienteNombre.Trim(),
                string.IsNullOrWhiteSpace(ClienteContacto) ? null : ClienteContacto.Trim(),
                FechaVigencia,
                Estado);

            try
            {
                await _cotizacionApiService.ActualizarAsync(Id, request);
                return RedirectToPage("/Cotizaciones/Detalle", new { id = Id });
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                ErrorMensaje = ex.Message;
                var cotizacion = await _cotizacionApiService.ObtenerPorIdAsync(Id);
                Folio = cotizacion?.Folio;
                return Page();
            }
        }
    }
}
