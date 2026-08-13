using Inventario.Core.Dtos;
using Inventario.Core.Enums;
using Inventario.Web.Services.Api;
using Inventario.Web.Services.Http;
using Inventario.Web.Services.Sesion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Cotizaciones
{
    public class DetalleModel : PageModel
    {
        private readonly ICotizacionApiService _cotizacionApiService;
        private readonly ICajaApiService _cajaApiService;
        private readonly ICurrentSessionAccessor _sesionActual;

        public DetalleModel(ICotizacionApiService cotizacionApiService, ICajaApiService cajaApiService, ICurrentSessionAccessor sesionActual)
        {
            _cotizacionApiService = cotizacionApiService;
            _cajaApiService = cajaApiService;
            _sesionActual = sesionActual;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty]
        public int CorteDeCajaId { get; set; }

        [BindProperty]
        public MetodoPago MetodoPago { get; set; } = MetodoPago.Efectivo;

        public CotizacionDto? Cotizacion { get; private set; }
        public IReadOnlyList<(CajaDto Caja, CorteDeCajaDto Corte)> CajasConCorteAbierto { get; private set; } = [];
        public string? ErrorMensaje { get; private set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var cotizacion = await _cotizacionApiService.ObtenerPorIdAsync(Id);
            if (cotizacion is null || cotizacion.InventarioId != _sesionActual.InventarioOperativoId)
            {
                return NotFound();
            }

            Cotizacion = cotizacion;

            if (cotizacion.Estado == EstadoCotizacion.Vigente)
            {
                await CargarCajasConCorteAbiertoAsync();
            }

            return Page();
        }

        public async Task<IActionResult> OnGetPdfAsync()
        {
            var cotizacion = await _cotizacionApiService.ObtenerPorIdAsync(Id);
            if (cotizacion is null || cotizacion.InventarioId != _sesionActual.InventarioOperativoId)
            {
                return NotFound();
            }

            var pdf = await _cotizacionApiService.ObtenerPdfAsync(Id);
            return File(pdf, "application/pdf", $"cotizacion-{cotizacion.Folio}.pdf");
        }

        public async Task<IActionResult> OnPostConvertirAsync()
        {
            var cotizacion = await _cotizacionApiService.ObtenerPorIdAsync(Id);
            if (cotizacion is null || cotizacion.InventarioId != _sesionActual.InventarioOperativoId)
            {
                return NotFound();
            }

            try
            {
                var request = new ConvertirAVentaRequest(_sesionActual.UsuarioId!.Value, CorteDeCajaId, MetodoPago);
                var venta = await _cotizacionApiService.ConvertirAVentaAsync(Id, request);
                return RedirectToPage("/Ventas/Detalle", new { id = venta.Id });
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                ErrorMensaje = ex.Message;
                Cotizacion = cotizacion;
                await CargarCajasConCorteAbiertoAsync();
                return Page();
            }
        }

        private async Task CargarCajasConCorteAbiertoAsync()
        {
            var inventarioId = _sesionActual.InventarioOperativoId!.Value;
            var cajas = await _cajaApiService.ObtenerPorInventarioAsync(inventarioId);

            var resultado = new List<(CajaDto, CorteDeCajaDto)>();
            foreach (var caja in cajas)
            {
                var corte = await _cajaApiService.ObtenerCorteAbiertoAsync(caja.Id);
                if (corte is not null)
                {
                    resultado.Add((caja, corte));
                }
            }

            CajasConCorteAbierto = resultado;
        }
    }
}
