using System.ComponentModel.DataAnnotations;
using Inventario.Core.Dtos;
using Inventario.Core.Enums;
using Inventario.Web.Services.Api;
using Inventario.Web.Services.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Usuarios
{
    [Authorize(Roles = "Administrador")]
    public class CrearModel : PageModel
    {
        private readonly IUsuarioApiService _usuarioApiService;
        private readonly IInventarioApiService _inventarioApiService;

        public CrearModel(IUsuarioApiService usuarioApiService, IInventarioApiService inventarioApiService)
        {
            _usuarioApiService = usuarioApiService;
            _inventarioApiService = inventarioApiService;
        }

        [BindProperty, Required(ErrorMessage = "El usuario es obligatorio."), StringLength(50)]
        public string NombreUsuario { get; set; } = string.Empty;

        [BindProperty, Required(ErrorMessage = "La contraseña es obligatoria."), MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string Password { get; set; } = string.Empty;

        [BindProperty, StringLength(200)]
        public string? NombreCompleto { get; set; }

        [BindProperty]
        public RolUsuario Rol { get; set; } = RolUsuario.Vendedor;

        [BindProperty]
        public List<int> InventarioIds { get; set; } = [];

        public IReadOnlyList<InventarioDto> InventariosDisponibles { get; private set; } = [];
        public string? ErrorMensaje { get; private set; }

        public async Task OnGetAsync() => await CargarInventariosAsync();

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await CargarInventariosAsync();
                return Page();
            }

            var request = new CrearUsuarioRequest(
                NombreUsuario.Trim(), Password,
                string.IsNullOrWhiteSpace(NombreCompleto) ? null : NombreCompleto.Trim(),
                Rol, InventarioIds);

            try
            {
                await _usuarioApiService.CrearAsync(request);
                return RedirectToPage("/Usuarios/Index");
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                ErrorMensaje = ex.Message;
                await CargarInventariosAsync();
                return Page();
            }
        }

        private async Task CargarInventariosAsync()
        {
            try
            {
                InventariosDisponibles = await _inventarioApiService.ObtenerTodosAsync();
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                ErrorMensaje = ex.Message;
            }
        }
    }
}
