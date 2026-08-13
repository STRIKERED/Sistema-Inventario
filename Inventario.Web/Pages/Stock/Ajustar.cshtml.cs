using System.ComponentModel.DataAnnotations;
using Inventario.Core.Dtos;
using Inventario.Core.Enums;
using Inventario.Web.Services.Api;
using Inventario.Web.Services.Http;
using Inventario.Web.Services.Sesion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Stock
{
    /// <summary>Alta manual de un movimiento de inventario (entrada/salida/ajuste). Reservado a
    /// Administrador/Gerente: mismo rol que exige StockController.RegistrarMovimiento en la Api.</summary>
    [Authorize(Roles = "Administrador,Gerente")]
    public class AjustarModel : PageModel
    {
        private readonly IProductoApiService _productoApiService;
        private readonly IStockApiService _stockApiService;
        private readonly ICurrentSessionAccessor _sesionActual;

        public AjustarModel(IProductoApiService productoApiService, IStockApiService stockApiService, ICurrentSessionAccessor sesionActual)
        {
            _productoApiService = productoApiService;
            _stockApiService = stockApiService;
            _sesionActual = sesionActual;
        }

        [BindProperty, Range(1, int.MaxValue, ErrorMessage = "Selecciona un producto.")]
        public int ProductoId { get; set; }

        [BindProperty]
        public TipoMovimientoInventario Tipo { get; set; } = TipoMovimientoInventario.Entrada;

        // Sin [Range]: para Entrada/Salida debe ser positiva, para Ajuste puede ser negativa (delta
        // con signo). Se valida a mano en OnPostAsync según el Tipo elegido — ver RegistrarMovimientoRequest.
        [BindProperty]
        public int Cantidad { get; set; }

        [BindProperty, StringLength(300)]
        public string? Motivo { get; set; }

        public IReadOnlyList<ProductoDto> Productos { get; private set; } = [];
        public string? ErrorMensaje { get; private set; }

        public async Task OnGetAsync() => await CargarProductosAsync();

        public async Task<IActionResult> OnPostAsync()
        {
            ValidarCantidad();

            if (!ModelState.IsValid)
            {
                await CargarProductosAsync();
                return Page();
            }

            var request = new RegistrarMovimientoRequest(
                ProductoId, Tipo, Cantidad,
                string.IsNullOrWhiteSpace(Motivo) ? null : Motivo.Trim(),
                _sesionActual.UsuarioId);

            try
            {
                await _stockApiService.RegistrarMovimientoAsync(request);
                return RedirectToPage("/Productos/Detalle", new { id = ProductoId });
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                ErrorMensaje = ex.Message;
                await CargarProductosAsync();
                return Page();
            }
        }

        private void ValidarCantidad()
        {
            if (Tipo == TipoMovimientoInventario.Ajuste)
            {
                if (Cantidad == 0)
                {
                    ModelState.AddModelError(nameof(Cantidad), "El ajuste no puede ser 0 (usa negativo para restar).");
                }
            }
            else if (Cantidad <= 0)
            {
                ModelState.AddModelError(nameof(Cantidad), "La cantidad debe ser mayor a 0.");
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
