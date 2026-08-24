using System.ComponentModel.DataAnnotations;
using Inventario.Core.Dtos;
using Inventario.Web.Services.Api;
using Inventario.Web.Services.Http;
using Inventario.Web.Services.Sesion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Reportes
{
    public class VentasPorPeriodoModel : PageModel
    {
        private readonly IVentaApiService _ventaApiService;
        private readonly ICurrentSessionAccessor _sesionActual;

        public VentasPorPeriodoModel(IVentaApiService ventaApiService, ICurrentSessionAccessor sesionActual)
        {
            _ventaApiService = ventaApiService;
            _sesionActual = sesionActual;
        }

        public record FilaInventario(InventarioDto Inventario, IReadOnlyList<VentaDto> Ventas);

        [BindProperty(SupportsGet = true), DataType(DataType.Date)]
        public DateTime? Desde { get; set; }

        [BindProperty(SupportsGet = true), DataType(DataType.Date)]
        public DateTime? Hasta { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool Consolidado { get; set; }

        public bool PuedeConsolidar => _sesionActual.InventariosDisponibles.Count > 1;

        public IReadOnlyList<FilaInventario> Resultados { get; private set; } = [];
        public decimal TotalGeneral => Resultados.SelectMany(r => r.Ventas).Sum(v => v.Total);
        public int CantidadGeneral => Resultados.Sum(r => r.Ventas.Count);
        public string? ErrorMensaje { get; private set; }

        public async Task OnGetAsync()
        {
            Desde ??= DateTime.Today.AddDays(-30);
            Hasta ??= DateTime.Today;

            var inventarios = Consolidado && PuedeConsolidar
                ? _sesionActual.InventariosDisponibles
                : _sesionActual.InventariosDisponibles.Where(i => i.Id == _sesionActual.InventarioOperativoId).ToList();

            try
            {
                var resultados = new List<FilaInventario>();
                foreach (var inventario in inventarios)
                {
                    var ventas = await _ventaApiService.ObtenerPorInventarioAsync(inventario.Id, Desde, Hasta);
                    resultados.Add(new FilaInventario(inventario, ventas));
                }

                Resultados = resultados;
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                ErrorMensaje = ex.Message;
            }
        }
    }
}
