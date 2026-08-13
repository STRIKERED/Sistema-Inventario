using Inventario.Core.Dtos;
using Inventario.Web.Services.Api;
using Inventario.Web.Services.Http;
using Inventario.Web.Services.Sesion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Caja
{
    public class HistorialModel : PageModel
    {
        private readonly ICajaApiService _cajaApiService;
        private readonly ICurrentSessionAccessor _sesionActual;

        public HistorialModel(ICajaApiService cajaApiService, ICurrentSessionAccessor sesionActual)
        {
            _cajaApiService = cajaApiService;
            _sesionActual = sesionActual;
        }

        [BindProperty(SupportsGet = true)]
        public int? CajaId { get; set; }

        public IReadOnlyList<CajaDto> Cajas { get; private set; } = [];
        public IReadOnlyList<CorteDeCajaDto> Cortes { get; private set; } = [];
        public string? ErrorMensaje { get; private set; }

        public async Task OnGetAsync()
        {
            var inventarioId = _sesionActual.InventarioOperativoId!.Value;

            try
            {
                Cajas = await _cajaApiService.ObtenerPorInventarioAsync(inventarioId);
                CajaId ??= Cajas.FirstOrDefault()?.Id;

                if (CajaId is not null)
                {
                    Cortes = (await _cajaApiService.ObtenerCortesPorCajaAsync(CajaId.Value))
                        .OrderByDescending(c => c.FechaApertura)
                        .ToList();
                }
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                ErrorMensaje = ex.Message;
            }
        }
    }
}
