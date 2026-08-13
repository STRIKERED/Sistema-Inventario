using Inventario.Core.Dtos;
using Inventario.Web.Services.Api;
using Inventario.Web.Services.Http;
using Inventario.Web.Services.Sesion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly IAuthApiService _authApiService;
        private readonly ISesionAuthService _sesionAuthService;
        private readonly ICurrentSessionAccessor _sesionActual;

        public LoginModel(IAuthApiService authApiService, ISesionAuthService sesionAuthService, ICurrentSessionAccessor sesionActual)
        {
            _authApiService = authApiService;
            _sesionAuthService = sesionAuthService;
            _sesionActual = sesionActual;
        }

        [BindProperty]
        public string NombreUsuario { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public string? MensajeError { get; set; }

        public IActionResult OnGet()
        {
            // Ya hay sesión activa (p. ej. volvió a /Auth/Login con el back del navegador): no tiene
            // caso mostrar el formulario otra vez.
            return _sesionActual.HaySesionActiva ? RedirigirDespuesDeIniciarSesion() : Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(NombreUsuario) || string.IsNullOrWhiteSpace(Password))
            {
                MensajeError = "Ingresa usuario y contraseña.";
                return Page();
            }

            try
            {
                var respuesta = await _authApiService.LoginAsync(new LoginRequest(NombreUsuario.Trim(), Password));
                await _sesionAuthService.IniciarSesionAsync(respuesta);
            }
            catch (ApiException ex)
            {
                // Se captura aquí a propósito: un 401 de /api/auth/login significa "usuario o
                // contraseña incorrectos", no "sesión vencida" — no debe pasar por
                // ManejoErroresApiFilter (que asume lo segundo y cerraría una sesión que ni existe).
                MensajeError = ex.Message;
                return Page();
            }

            return RedirigirDespuesDeIniciarSesion();
        }

        private IActionResult RedirigirDespuesDeIniciarSesion()
        {
            if (_sesionActual.DebeSeleccionarInventario)
            {
                return RedirectToPage("/Auth/SeleccionarInventario", new { returnUrl = ReturnUrl });
            }

            if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            {
                return LocalRedirect(ReturnUrl);
            }

            return RedirectToPage("/Dashboard/Index");
        }
    }
}
