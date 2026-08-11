using System.Net.Http.Json;
using Inventario.Core.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Productos
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<Producto> Productos { get; private set; } = new();
        public string? ErrorMensaje { get; private set; }

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("InventarioApi");

            try
            {
                var productos = await client.GetFromJsonAsync<List<Producto>>("api/productos");
                Productos = productos ?? new List<Producto>();
            }
            catch (HttpRequestException ex)
            {
                ErrorMensaje = $"No se pudo conectar con Inventario.Api: {ex.Message}";
            }
        }
    }
}
