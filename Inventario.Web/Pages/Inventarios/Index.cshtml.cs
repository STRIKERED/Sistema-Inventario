using System.ComponentModel.DataAnnotations;
using Inventario.Core.Dtos;
using Inventario.Web.Services.Api;
using Inventario.Web.Services.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Inventarios
{
    /// <summary>Alta y baja de Inventarios (p. ej. "Papelería"/"Abarrotes" dentro de una Sucursal).
    /// Solo Administrador — la Api también lo restringe en POST/PUT.</summary>
    [Authorize(Roles = "Administrador")]
    public class IndexModel : PageModel
    {
        private readonly IInventarioApiService _inventarioApiService;
        private readonly ISucursalApiService _sucursalApiService;

        public IndexModel(IInventarioApiService inventarioApiService, ISucursalApiService sucursalApiService)
        {
            _inventarioApiService = inventarioApiService;
            _sucursalApiService = sucursalApiService;
        }

        [BindProperty, Required(ErrorMessage = "El nombre es obligatorio."), StringLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [BindProperty, Range(1, int.MaxValue, ErrorMessage = "Selecciona una sucursal.")]
        public int SucursalId { get; set; }

        [BindProperty]
        public int InventarioId { get; set; }

        public IReadOnlyList<InventarioDto> Inventarios { get; private set; } = [];
        public IReadOnlyList<SucursalDto> Sucursales { get; private set; } = [];
        public string? ErrorMensaje { get; private set; }

        public async Task OnGetAsync() => await CargarAsync();

        public async Task<IActionResult> OnPostCrearAsync()
        {
            if (!ModelState.IsValid)
            {
                await CargarAsync();
                return Page();
            }

            try
            {
                await _inventarioApiService.CrearAsync(new InventarioRequest(Nombre.Trim(), SucursalId, true));
                return RedirectToPage();
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                ErrorMensaje = ex.Message;
                await CargarAsync();
                return Page();
            }
        }

        // Alta/baja rápida desde la lista, mismo patrón que Inventario.Desktop.InventariosViewModel.
        public async Task<IActionResult> OnPostCambiarActivoAsync()
        {
            try
            {
                var actual = await _inventarioApiService.ObtenerPorIdAsync(InventarioId);
                if (actual is null)
                {
                    return NotFound();
                }

                await _inventarioApiService.ActualizarAsync(InventarioId, new InventarioRequest(actual.Nombre, actual.SucursalId, !actual.Activo));
                return RedirectToPage();
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                ErrorMensaje = ex.Message;
                await CargarAsync();
                return Page();
            }
        }

        private async Task CargarAsync()
        {
            try
            {
                Inventarios = await _inventarioApiService.ObtenerTodosAsync();
                Sucursales = await _sucursalApiService.ObtenerTodasAsync();
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                ErrorMensaje = ex.Message;
            }
        }
    }
}
