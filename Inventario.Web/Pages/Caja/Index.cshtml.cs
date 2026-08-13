using Inventario.Core.Dtos;
using Inventario.Web.Services.Api;
using Inventario.Web.Services.Http;
using Inventario.Web.Services.Sesion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Caja
{
    /// <summary>Estado de caja actual del Inventario activo: lista de Cajas con su corte abierto (si
    /// lo hay), y formularios inline para abrir/cerrar turno o dar de alta una caja nueva.</summary>
    public class IndexModel : PageModel
    {
        private readonly ICajaApiService _cajaApiService;
        private readonly ICurrentSessionAccessor _sesionActual;

        public IndexModel(ICajaApiService cajaApiService, ICurrentSessionAccessor sesionActual)
        {
            _cajaApiService = cajaApiService;
            _sesionActual = sesionActual;
        }

        public IReadOnlyList<(CajaDto Caja, CorteDeCajaDto? CorteAbierto)> Cajas { get; private set; } = [];
        public string? ErrorMensaje { get; private set; }

        [BindProperty]
        public int CajaId { get; set; }

        [BindProperty]
        public decimal MontoInicial { get; set; }

        [BindProperty]
        public int CorteDeCajaId { get; set; }

        [BindProperty]
        public decimal MontoFinalContado { get; set; }

        [BindProperty]
        public string NuevaCajaNombre { get; set; } = string.Empty;

        public async Task OnGetAsync() => await CargarAsync();

        public async Task<IActionResult> OnPostAbrirAsync()
        {
            try
            {
                await _cajaApiService.AbrirCorteAsync(new AbrirCorteRequest(CajaId, _sesionActual.UsuarioId!.Value, MontoInicial));
                return RedirectToPage();
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                ErrorMensaje = ex.Message;
                await CargarAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostCerrarAsync()
        {
            try
            {
                await _cajaApiService.CerrarCorteAsync(CorteDeCajaId, new CerrarCorteRequest(MontoFinalContado));
                return RedirectToPage();
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                ErrorMensaje = ex.Message;
                await CargarAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostNuevaCajaAsync()
        {
            if (string.IsNullOrWhiteSpace(NuevaCajaNombre))
            {
                ErrorMensaje = "El nombre de la caja es obligatorio.";
                await CargarAsync();
                return Page();
            }

            try
            {
                await _cajaApiService.CrearAsync(new CajaRequest(NuevaCajaNombre.Trim(), _sesionActual.InventarioOperativoId!.Value));
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
            var inventarioId = _sesionActual.InventarioOperativoId!.Value;

            try
            {
                var cajas = await _cajaApiService.ObtenerPorInventarioAsync(inventarioId);
                var resultado = new List<(CajaDto, CorteDeCajaDto?)>();
                foreach (var caja in cajas)
                {
                    resultado.Add((caja, await _cajaApiService.ObtenerCorteAbiertoAsync(caja.Id)));
                }

                Cajas = resultado;
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                ErrorMensaje = ex.Message;
            }
        }
    }
}
