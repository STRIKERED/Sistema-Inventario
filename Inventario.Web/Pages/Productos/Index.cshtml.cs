using Inventario.Core.Dtos;
using Inventario.Web.Services.Api;
using Inventario.Web.Services.Http;
using Inventario.Web.Services.Sesion;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Productos
{
    public class IndexModel : PageModel
    {
        private readonly IProductoApiService _productoApiService;
        private readonly ICurrentSessionAccessor _sesionActual;

        public IndexModel(IProductoApiService productoApiService, ICurrentSessionAccessor sesionActual)
        {
            _productoApiService = productoApiService;
            _sesionActual = sesionActual;
        }

        public IReadOnlyList<ProductoDto> Productos { get; private set; } = [];
        public string? ErrorMensaje { get; private set; }

        public async Task OnGetAsync()
        {
            var inventarioId = _sesionActual.InventarioOperativoId!.Value;

            try
            {
                Productos = await _productoApiService.ObtenerPorInventarioAsync(inventarioId);
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                ErrorMensaje = ex.Message;
            }
        }
    }
}
