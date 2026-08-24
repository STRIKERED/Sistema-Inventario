using System.ComponentModel.DataAnnotations;
using Inventario.Web.Services.Api;
using Inventario.Web.Services.Http;
using Inventario.Web.Services.Sesion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Reportes
{
    /// <summary>Índice de rotación simplificado: unidades vendidas en el periodo / stock actual. Un
    /// producto sin stock que sí vendió muestra "—" (no se puede dividir entre cero) pero se ordena
    /// primero, junto con el resto de índices más altos.</summary>
    public class RotacionInventarioModel : PageModel
    {
        private readonly IVentaApiService _ventaApiService;
        private readonly IProductoApiService _productoApiService;
        private readonly ICurrentSessionAccessor _sesionActual;

        public RotacionInventarioModel(IVentaApiService ventaApiService, IProductoApiService productoApiService, ICurrentSessionAccessor sesionActual)
        {
            _ventaApiService = ventaApiService;
            _productoApiService = productoApiService;
            _sesionActual = sesionActual;
        }

        public record Fila(string Producto, int UnidadesVendidas, int StockActual, decimal? IndiceRotacion);

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

            var inventarioId = _sesionActual.InventarioOperativoId!.Value;

            try
            {
                var productosTask = _productoApiService.ObtenerPorInventarioAsync(inventarioId);
                var ventasTask = _ventaApiService.ObtenerPorInventarioAsync(inventarioId, Desde, Hasta);
                await Task.WhenAll(productosTask, ventasTask);

                var vendidosPorProducto = (await ventasTask)
                    .SelectMany(v => v.Detalles)
                    .GroupBy(d => d.ProductoId)
                    .ToDictionary(g => g.Key, g => g.Sum(d => d.Cantidad));

                Filas = (await productosTask)
                    .Where(p => p.Activo)
                    .Select(p =>
                    {
                        var vendidas = vendidosPorProducto.GetValueOrDefault(p.Id, 0);
                        decimal? indice = p.CantidadDisponible > 0 ? Math.Round((decimal)vendidas / p.CantidadDisponible, 2) : null;
                        return new Fila(p.Nombre, vendidas, p.CantidadDisponible, indice);
                    })
                    .OrderByDescending(f => f.IndiceRotacion ?? (f.UnidadesVendidas > 0 ? decimal.MaxValue : -1))
                    .ToList();
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                ErrorMensaje = ex.Message;
            }
        }
    }
}
