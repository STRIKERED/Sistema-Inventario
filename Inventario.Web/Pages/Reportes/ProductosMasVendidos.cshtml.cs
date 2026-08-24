using System.ComponentModel.DataAnnotations;
using Inventario.Web.Services.Api;
using Inventario.Web.Services.Http;
using Inventario.Web.Services.Sesion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Reportes
{
    public class ProductosMasVendidosModel : PageModel
    {
        private readonly IVentaApiService _ventaApiService;
        private readonly ICurrentSessionAccessor _sesionActual;

        public ProductosMasVendidosModel(IVentaApiService ventaApiService, ICurrentSessionAccessor sesionActual)
        {
            _ventaApiService = ventaApiService;
            _sesionActual = sesionActual;
        }

        public record Fila(string Producto, int CantidadVendida, decimal ImporteTotal);

        [BindProperty(SupportsGet = true), DataType(DataType.Date)]
        public DateTime? Desde { get; set; }

        [BindProperty(SupportsGet = true), DataType(DataType.Date)]
        public DateTime? Hasta { get; set; }

        public IReadOnlyList<Fila> Filas { get; private set; } = [];
        public string? ErrorMensaje { get; private set; }

        public async Task OnGetAsync()
        {
            Desde ??= DateTime.Today.AddDays(-30);
            Hasta ??= DateTime.Today;

            try
            {
                var ventas = await _ventaApiService.ObtenerPorInventarioAsync(_sesionActual.InventarioOperativoId!.Value, Desde, Hasta);

                Filas = ventas
                    .SelectMany(v => v.Detalles)
                    .GroupBy(d => d.ProductoNombre ?? $"Producto #{d.ProductoId}")
                    .Select(g => new Fila(g.Key, g.Sum(d => d.Cantidad), g.Sum(d => d.ImporteLinea)))
                    .OrderByDescending(f => f.CantidadVendida)
                    .ToList();
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                ErrorMensaje = ex.Message;
            }
        }
    }
}
