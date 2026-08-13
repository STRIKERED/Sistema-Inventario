using Inventario.Web.Services.Sesion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Auth
{
    public class LogoutModel : PageModel
    {
        private readonly ISesionAuthService _sesionAuthService;

        public LogoutModel(ISesionAuthService sesionAuthService)
        {
            _sesionAuthService = sesionAuthService;
        }

        public async Task<IActionResult> OnGetAsync() => await CerrarYRedirigirAsync();

        public async Task<IActionResult> OnPostAsync() => await CerrarYRedirigirAsync();

        private async Task<IActionResult> CerrarYRedirigirAsync()
        {
            await _sesionAuthService.CerrarSesionAsync();
            return RedirectToPage("/Auth/Login");
        }
    }
}
