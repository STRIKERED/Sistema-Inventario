using System.ComponentModel.DataAnnotations;
using Inventario.Core.Dtos;
using Inventario.Web.Services.Api;
using Inventario.Web.Services.Http;
using Inventario.Web.Services.Sesion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Cotizaciones
{
    /// <summary>Alta de cotización. Sin JavaScript para agregar/quitar líneas dinámicamente: el
    /// formulario siempre muestra <see cref="MaximoLineas"/> filas y descarta al guardar las que
    /// queden sin producto o con cantidad 0 — suficiente para el volumen típico de una cotización.</summary>
    public class CrearModel : PageModel
    {
        private const int MaximoLineasConst = 8;

        private readonly ICotizacionApiService _cotizacionApiService;
        private readonly IProductoApiService _productoApiService;
        private readonly ICurrentSessionAccessor _sesionActual;

        public CrearModel(ICotizacionApiService cotizacionApiService, IProductoApiService productoApiService, ICurrentSessionAccessor sesionActual)
        {
            _cotizacionApiService = cotizacionApiService;
            _productoApiService = productoApiService;
            _sesionActual = sesionActual;
        }

        public int MaximoLineas => MaximoLineasConst;

        [BindProperty, StringLength(200)]
        public string? ClienteNombre { get; set; }

        [BindProperty, StringLength(200)]
        public string? ClienteContacto { get; set; }

        [BindProperty, DataType(DataType.Date)]
        public DateTime? FechaVigencia { get; set; }

        [BindProperty, Range(0, double.MaxValue, ErrorMessage = "El descuento no puede ser negativo.")]
        public decimal Descuento { get; set; }

        [BindProperty]
        public List<int?> LineaProductoId { get; set; } = [];

        [BindProperty]
        public List<int> LineaCantidad { get; set; } = [];

        public IReadOnlyList<ProductoDto> Productos { get; private set; } = [];
        public string? ErrorMensaje { get; private set; }

        public async Task OnGetAsync() => await CargarProductosAsync();

        public async Task<IActionResult> OnPostAsync()
        {
            var detalles = new List<DetalleCotizacionRequest>();
            for (var i = 0; i < LineaProductoId.Count; i++)
            {
                var productoId = LineaProductoId[i];
                var cantidad = i < LineaCantidad.Count ? LineaCantidad[i] : 0;

                if (productoId is > 0 && cantidad > 0)
                {
                    detalles.Add(new DetalleCotizacionRequest(productoId.Value, cantidad));
                }
            }

            if (detalles.Count == 0)
            {
                ErrorMensaje = "Agrega al menos un producto con cantidad mayor a 0.";
                await CargarProductosAsync();
                return Page();
            }

            var request = new CrearCotizacionRequest(
                string.IsNullOrWhiteSpace(ClienteNombre) ? null : ClienteNombre.Trim(),
                string.IsNullOrWhiteSpace(ClienteContacto) ? null : ClienteContacto.Trim(),
                _sesionActual.InventarioOperativoId!.Value,
                _sesionActual.UsuarioId!.Value,
                FechaVigencia,
                Descuento,
                detalles);

            try
            {
                var creada = await _cotizacionApiService.CrearAsync(request);
                return RedirectToPage("/Cotizaciones/Detalle", new { id = creada.Id });
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                ErrorMensaje = ex.Message;
                await CargarProductosAsync();
                return Page();
            }
        }

        private async Task CargarProductosAsync()
        {
            var inventarioId = _sesionActual.InventarioOperativoId!.Value;

            try
            {
                Productos = (await _productoApiService.ObtenerPorInventarioAsync(inventarioId))
                    .Where(p => p.Activo)
                    .OrderBy(p => p.Nombre)
                    .ToList();
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                ErrorMensaje = ex.Message;
            }
        }
    }
}
