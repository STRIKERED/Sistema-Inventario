using Inventario.Core.Dtos;
using Inventario.Web.Services.Api;
using Inventario.Web.Services.Sesion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Caja
{
    public class DetalleCorteModel : PageModel
    {
        private readonly ICajaApiService _cajaApiService;
        private readonly IVentaApiService _ventaApiService;
        private readonly ICurrentSessionAccessor _sesionActual;

        public DetalleCorteModel(ICajaApiService cajaApiService, IVentaApiService ventaApiService, ICurrentSessionAccessor sesionActual)
        {
            _cajaApiService = cajaApiService;
            _ventaApiService = ventaApiService;
            _sesionActual = sesionActual;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public CorteDeCajaDto? Corte { get; private set; }
        public IReadOnlyList<VentaDto> Ventas { get; private set; } = [];

        public async Task<IActionResult> OnGetAsync()
        {
            var corte = await _cajaApiService.ObtenerCortePorIdAsync(Id);
            if (corte is null)
            {
                return NotFound();
            }

            // ObtenerCortePorIdAsync no filtra por Inventario (el corte no lo trae directo, solo vía
            // Caja); se resuelve verificando que la caja del corte esté entre las del Inventario activo.
            var cajasDelInventario = await _cajaApiService.ObtenerPorInventarioAsync(_sesionActual.InventarioOperativoId!.Value);
            if (cajasDelInventario.All(c => c.Id != corte.CajaId))
            {
                return NotFound();
            }

            Corte = corte;
            Ventas = await _ventaApiService.ObtenerPorCorteDeCajaAsync(Id);

            return Page();
        }
    }
}
