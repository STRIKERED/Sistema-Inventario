using System.ComponentModel.DataAnnotations;
using Inventario.Core.Dtos;
using Inventario.Web.Services.Api;
using Inventario.Web.Services.Http;
using Inventario.Web.Services.Sesion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Productos
{
    public class EditarModel : PageModel
    {
        private readonly IProductoApiService _productoApiService;
        private readonly ICurrentSessionAccessor _sesionActual;

        public EditarModel(IProductoApiService productoApiService, ICurrentSessionAccessor sesionActual)
        {
            _productoApiService = productoApiService;
            _sesionActual = sesionActual;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty, Required(ErrorMessage = "El SKU es obligatorio."), StringLength(50)]
        public string Sku { get; set; } = string.Empty;

        [BindProperty, Required(ErrorMessage = "El código de barras es obligatorio."), StringLength(50)]
        public string CodigoBarras { get; set; } = string.Empty;

        [BindProperty, Required(ErrorMessage = "El nombre es obligatorio."), StringLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [BindProperty, StringLength(100)]
        public string? Categoria { get; set; }

        [BindProperty, StringLength(50)]
        public string? Unidad { get; set; }

        [BindProperty, Range(0, double.MaxValue, ErrorMessage = "El precio de costo no puede ser negativo.")]
        public decimal PrecioCosto { get; set; }

        [BindProperty, Range(0, double.MaxValue, ErrorMessage = "El precio de venta no puede ser negativo.")]
        public decimal PrecioVenta { get; set; }

        [BindProperty, Range(0, int.MaxValue, ErrorMessage = "La cantidad disponible no puede ser negativa.")]
        public int CantidadDisponible { get; set; }

        [BindProperty, Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo.")]
        public int StockMinimo { get; set; }

        [BindProperty]
        public bool Activo { get; set; }

        public string? ErrorMensaje { get; private set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var producto = await _productoApiService.ObtenerPorIdAsync(Id);

            // También rechaza productos de otro Inventario: evita editar por URL algo fuera del
            // alcance del Inventario con el que se está operando.
            if (producto is null || producto.InventarioId != _sesionActual.InventarioOperativoId)
            {
                return NotFound();
            }

            Sku = producto.Sku;
            CodigoBarras = producto.CodigoBarras;
            Nombre = producto.Nombre;
            Categoria = producto.Categoria;
            Unidad = producto.Unidad;
            PrecioCosto = producto.PrecioCosto;
            PrecioVenta = producto.PrecioVenta;
            CantidadDisponible = producto.CantidadDisponible;
            StockMinimo = producto.StockMinimo;
            Activo = producto.Activo;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var inventarioId = _sesionActual.InventarioOperativoId!.Value;
            var request = new ActualizarProductoRequest(
                Sku.Trim(),
                CodigoBarras.Trim(),
                Nombre.Trim(),
                string.IsNullOrWhiteSpace(Categoria) ? null : Categoria.Trim(),
                string.IsNullOrWhiteSpace(Unidad) ? null : Unidad.Trim(),
                PrecioCosto,
                PrecioVenta,
                Activo,
                inventarioId,
                CantidadDisponible,
                StockMinimo);

            try
            {
                await _productoApiService.ActualizarAsync(Id, request);
                return RedirectToPage("/Productos/Detalle", new { id = Id });
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                ErrorMensaje = ex.Message;
                return Page();
            }
        }
    }
}
