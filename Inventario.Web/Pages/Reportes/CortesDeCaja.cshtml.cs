using System.ComponentModel.DataAnnotations;
using Inventario.Core.Dtos;
using Inventario.Web.Services.Api;
using Inventario.Web.Services.Http;
using Inventario.Web.Services.Sesion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Reportes
{
    public class CortesDeCajaModel : PageModel
    {
        private readonly ICajaApiService _cajaApiService;
        private readonly ICurrentSessionAccessor _sesionActual;

        public CortesDeCajaModel(ICajaApiService cajaApiService, ICurrentSessionAccessor sesionActual)
        {
            _cajaApiService = cajaApiService;
            _sesionActual = sesionActual;
        }

        [BindProperty(SupportsGet = true), DataType(DataType.Date)]
        public DateTime? Desde { get; set; }

        [BindProperty(SupportsGet = true), DataType(DataType.Date)]
        public DateTime? Hasta { get; set; }

        public IReadOnlyList<CorteDeCajaDto> Cortes { get; private set; } = [];
        public decimal TotalDiferencias => Cortes.Sum(c => c.Diferencia);
        public string? ErrorMensaje { get; private set; }

        public async Task OnGetAsync()
        {
            Desde ??= DateTime.Today.AddDays(-30);
            Hasta ??= DateTime.Today;

            // FechaApertura se guarda en UTC (DateTime.UtcNow); Desde/Hasta son fechas de calendario
            // locales (vienen de un <input type="date">). Hay que convertirlas a UTC antes de
            // comparar, o el filtro queda mal en cualquier zona horaria distinta de UTC — mismo bug
            // que ya se corrigió del lado de la Api para Ventas/Movimientos (RangoFechaLocalHelper).
            var desdeUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(Desde.Value.Date, DateTimeKind.Unspecified), TimeZoneInfo.Local);
            var hastaUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(Hasta.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Unspecified), TimeZoneInfo.Local);

            var inventarioId = _sesionActual.InventarioOperativoId!.Value;

            try
            {
                var cajas = await _cajaApiService.ObtenerPorInventarioAsync(inventarioId);

                var cortes = new List<CorteDeCajaDto>();
                foreach (var caja in cajas)
                {
                    var cortesCaja = await _cajaApiService.ObtenerCortesPorCajaAsync(caja.Id);
                    cortes.AddRange(cortesCaja.Where(c => c.FechaApertura >= desdeUtc && c.FechaApertura <= hastaUtc));
                }

                Cortes = cortes.OrderByDescending(c => c.FechaApertura).ToList();
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                ErrorMensaje = ex.Message;
            }
        }
    }
}
