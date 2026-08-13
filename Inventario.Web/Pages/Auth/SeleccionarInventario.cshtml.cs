using Inventario.Core.Dtos;
using Inventario.Web.Services.Sesion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Auth
{
    /// <summary>
    /// Elegir con qué Inventario operar. Se llega aquí tras el login si el usuario tiene acceso a
    /// más de uno (RequiereInventarioSeleccionadoFilter redirige aquí desde cualquier otra página
    /// mientras no haya elegido), y también desde el selector persistente del layout para cambiar de
    /// Inventario en cualquier momento de la sesión.
    /// </summary>
    public class SeleccionarInventarioModel : PageModel
    {
        private readonly ISesionAuthService _sesionAuthService;
        private readonly ICurrentSessionAccessor _sesionActual;

        public SeleccionarInventarioModel(ISesionAuthService sesionAuthService, ICurrentSessionAccessor sesionActual)
        {
            _sesionAuthService = sesionAuthService;
            _sesionActual = sesionActual;
        }

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        [BindProperty]
        public int InventarioId { get; set; }

        public string? MensajeError { get; set; }

        public IReadOnlyList<InventarioDto> Inventarios => _sesionActual.InventariosDisponibles;

        public async Task<IActionResult> OnGetAsync()
        {
            if (!_sesionActual.HaySesionActiva)
            {
                return RedirectToPage("/Auth/Login", new { returnUrl = ReturnUrl });
            }

            // Un solo Inventario disponible: no hace falta preguntar, se fija solo. Cubre tanto el
            // primer login como volver aquí manualmente por el link "Cambiar" del layout.
            if (Inventarios.Count == 1)
            {
                await _sesionAuthService.FijarInventarioOperativoAsync(Inventarios[0].Id);
                return Redirigir();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!await _sesionAuthService.FijarInventarioOperativoAsync(InventarioId))
            {
                MensajeError = "No tienes acceso a ese Inventario.";
                return Page();
            }

            return Redirigir();
        }

        private IActionResult Redirigir()
        {
            if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            {
                return LocalRedirect(ReturnUrl);
            }

            return RedirectToPage("/Dashboard/Index");
        }
    }
}
