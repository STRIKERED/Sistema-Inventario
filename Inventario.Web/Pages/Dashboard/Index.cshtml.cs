using Inventario.Core.Dtos;
using Inventario.Web.Services.Api;
using Inventario.Web.Services.Sesion;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Dashboard
{
    /// <summary>Resumen del Inventario activo: ventas de hoy, stock bajo, estado de caja y
    /// cotizaciones por vencer. RequiereInventarioSeleccionadoFilter garantiza que al llegar aquí ya
    /// hay un InventarioOperativoId elegido.</summary>
    public class IndexModel : PageModel
    {
        private readonly IVentaApiService _ventaApiService;
        private readonly IProductoApiService _productoApiService;
        private readonly ICajaApiService _cajaApiService;
        private readonly ICotizacionApiService _cotizacionApiService;
        private readonly ICurrentSessionAccessor _sesionActual;

        public IndexModel(
            IVentaApiService ventaApiService,
            IProductoApiService productoApiService,
            ICajaApiService cajaApiService,
            ICotizacionApiService cotizacionApiService,
            ICurrentSessionAccessor sesionActual)
        {
            _ventaApiService = ventaApiService;
            _productoApiService = productoApiService;
            _cajaApiService = cajaApiService;
            _cotizacionApiService = cotizacionApiService;
            _sesionActual = sesionActual;
        }

        public IReadOnlyList<VentaDto> VentasDeHoy { get; private set; } = [];
        public decimal TotalVentasDeHoy => VentasDeHoy.Sum(v => v.Total);

        public IReadOnlyList<ProductoDto> ProductosConStockBajo { get; private set; } = [];

        public IReadOnlyList<(CajaDto Caja, CorteDeCajaDto? CorteAbierto)> Cajas { get; private set; } = [];
        public int CajasAbiertas => Cajas.Count(c => c.CorteAbierto is not null);

        public IReadOnlyList<CotizacionDto> CotizacionesVigentes { get; private set; } = [];

        public string? ErrorMensaje { get; private set; }

        public async Task OnGetAsync()
        {
            var inventarioId = _sesionActual.InventarioOperativoId!.Value;

            try
            {
                var productosTask = _productoApiService.ObtenerPorInventarioAsync(inventarioId);
                var ventasTask = _ventaApiService.ObtenerPorInventarioAsync(inventarioId);
                var cajasTask = _cajaApiService.ObtenerPorInventarioAsync(inventarioId);
                var cotizacionesTask = _cotizacionApiService.ObtenerVigentesAsync(inventarioId);

                await Task.WhenAll(productosTask, ventasTask, cajasTask, cotizacionesTask);

                ProductosConStockBajo = (await productosTask)
                    .Where(p => p.Activo && p.CantidadDisponible <= p.StockMinimo)
                    .OrderBy(p => p.CantidadDisponible)
                    .ToList();

                VentasDeHoy = await ventasTask;
                CotizacionesVigentes = await cotizacionesTask;

                var cajas = await cajasTask;
                var cajasConCorte = new List<(CajaDto, CorteDeCajaDto?)>();
                foreach (var caja in cajas)
                {
                    cajasConCorte.Add((caja, await _cajaApiService.ObtenerCorteAbiertoAsync(caja.Id)));
                }

                Cajas = cajasConCorte;
            }
            catch (Services.Http.ApiException ex) when (ex.StatusCode != 401)
            {
                // 401 se deja escapar a propósito: lo atrapa ManejoErroresApiFilter, que cierra la
                // sesión (JWT vencido/inválido) y redirige a Login. Cualquier otro error se muestra aquí.
                ErrorMensaje = ex.Message;
            }
        }
    }
}
