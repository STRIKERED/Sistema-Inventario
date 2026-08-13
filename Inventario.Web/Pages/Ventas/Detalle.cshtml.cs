using Inventario.Core.Dtos;
using Inventario.Web.Services.Api;
using Inventario.Web.Services.Sesion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Ventas
{
    public class DetalleModel : PageModel
    {
        private readonly IVentaApiService _ventaApiService;
        private readonly ICurrentSessionAccessor _sesionActual;

        public DetalleModel(IVentaApiService ventaApiService, ICurrentSessionAccessor sesionActual)
        {
            _ventaApiService = ventaApiService;
            _sesionActual = sesionActual;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public VentaDto? Venta { get; private set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var venta = await _ventaApiService.ObtenerPorIdAsync(Id);

            // No se puede ver por URL una venta de otro Inventario.
            if (venta is null || venta.InventarioId != _sesionActual.InventarioOperativoId)
            {
                return NotFound();
            }

            Venta = venta;
            return Page();
        }

        /// <summary>Descarga el flujo de bytes ESC/POS crudo (útil para depurar o imprimir manualmente
        /// desde otro equipo; ver ITicketPrintService en la Api).</summary>
        public async Task<IActionResult> OnGetTicketAsync()
        {
            var venta = await _ventaApiService.ObtenerPorIdAsync(Id);
            if (venta is null || venta.InventarioId != _sesionActual.InventarioOperativoId)
            {
                return NotFound();
            }

            var bytes = await _ventaApiService.ObtenerTicketAsync(Id);
            return File(bytes, "application/octet-stream", $"ticket-{venta.Folio}.bin");
        }
    }
}
