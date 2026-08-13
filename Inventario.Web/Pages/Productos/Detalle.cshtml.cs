using Inventario.Core.Dtos;
using Inventario.Web.Services.Api;
using Inventario.Web.Services.Http;
using Inventario.Web.Services.Sesion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Productos
{
    public class DetalleModel : PageModel
    {
        private readonly IProductoApiService _productoApiService;
        private readonly IStockApiService _stockApiService;
        private readonly ICurrentSessionAccessor _sesionActual;

        public DetalleModel(IProductoApiService productoApiService, IStockApiService stockApiService, ICurrentSessionAccessor sesionActual)
        {
            _productoApiService = productoApiService;
            _stockApiService = stockApiService;
            _sesionActual = sesionActual;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public ProductoDto? Producto { get; private set; }
        public IReadOnlyList<MovimientoInventarioDto> Movimientos { get; private set; } = [];
        public string? ErrorMensaje { get; private set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var producto = await _productoApiService.ObtenerPorIdAsync(Id);

            // Igual que en Editar: un producto de otro Inventario no debe poder verse por URL.
            if (producto is null || producto.InventarioId != _sesionActual.InventarioOperativoId)
            {
                return NotFound();
            }

            Producto = producto;

            try
            {
                Movimientos = await _stockApiService.ObtenerMovimientosPorProductoAsync(Id);
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                ErrorMensaje = ex.Message;
            }

            return Page();
        }
    }
}
