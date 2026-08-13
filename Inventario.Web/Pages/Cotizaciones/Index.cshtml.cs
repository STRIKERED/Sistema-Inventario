using Inventario.Core.Dtos;
using Inventario.Web.Services.Api;
using Inventario.Web.Services.Http;
using Inventario.Web.Services.Sesion;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Cotizaciones
{
    public class IndexModel : PageModel
    {
        private readonly ICotizacionApiService _cotizacionApiService;
        private readonly ICurrentSessionAccessor _sesionActual;

        public IndexModel(ICotizacionApiService cotizacionApiService, ICurrentSessionAccessor sesionActual)
        {
            _cotizacionApiService = cotizacionApiService;
            _sesionActual = sesionActual;
        }

        public IReadOnlyList<CotizacionDto> Cotizaciones { get; private set; } = [];
        public string? ErrorMensaje { get; private set; }

        public async Task OnGetAsync()
        {
            var inventarioId = _sesionActual.InventarioOperativoId!.Value;

            try
            {
                Cotizaciones = await _cotizacionApiService.ObtenerPorInventarioAsync(inventarioId);
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                ErrorMensaje = ex.Message;
            }
        }
    }
}
