using System.ComponentModel.DataAnnotations;
using Inventario.Core.Dtos;
using Inventario.Web.Services.Api;
using Inventario.Web.Services.Http;
using Inventario.Web.Services.Sesion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Ventas
{
    /// <summary>Historial de ventas del Inventario activo. El mismo toggle "Solo canceladas" cubre
    /// lo que el spec original pedía como una pantalla aparte (mismo patrón que Stock/Index con
    /// "Solo stock bajo").</summary>
    public class IndexModel : PageModel
    {
        private readonly IVentaApiService _ventaApiService;
        private readonly ICurrentSessionAccessor _sesionActual;

        public IndexModel(IVentaApiService ventaApiService, ICurrentSessionAccessor sesionActual)
        {
            _ventaApiService = ventaApiService;
            _sesionActual = sesionActual;
        }

        [BindProperty(SupportsGet = true), DataType(DataType.Date)]
        public DateTime? Desde { get; set; }

        [BindProperty(SupportsGet = true), DataType(DataType.Date)]
        public DateTime? Hasta { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool SoloCanceladas { get; set; }

        public IReadOnlyList<VentaDto> Ventas { get; private set; } = [];
        public string? ErrorMensaje { get; private set; }

        public async Task OnGetAsync()
        {
            // Default: últimos 30 días, para no traer todo el historial de golpe la primera vez.
            Desde ??= DateTime.Today.AddDays(-30);
            Hasta ??= DateTime.Today;

            var inventarioId = _sesionActual.InventarioOperativoId!.Value;

            try
            {
                Ventas = await _ventaApiService.ObtenerPorInventarioAsync(inventarioId, Desde, Hasta, SoloCanceladas);
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                ErrorMensaje = ex.Message;
            }
        }
    }
}
