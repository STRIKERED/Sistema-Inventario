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
    public class EditarModel : PageModel
    {
        private readonly IUsuarioApiService _usuarioApiService;
        private readonly IInventarioApiService _inventarioApiService;

        public EditarModel(IUsuarioApiService usuarioApiService, IInventarioApiService inventarioApiService)
        {
            _usuarioApiService = usuarioApiService;
            _inventarioApiService = inventarioApiService;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty, Required(ErrorMessage = "El usuario es obligatorio."), StringLength(50)]
        public string NombreUsuario { get; set; } = string.Empty;

        [BindProperty, StringLength(200)]
        public string? NombreCompleto { get; set; }

        [BindProperty]
        public RolUsuario Rol { get; set; }

        [BindProperty]
        public bool Activo { get; set; }

        [BindProperty]
        public List<int> InventarioIds { get; set; } = [];

        public IReadOnlyList<InventarioDto> InventariosDisponibles { get; private set; } = [];
        public string? ErrorMensaje { get; private set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var usuario = await _usuarioApiService.ObtenerPorIdAsync(Id);
            if (usuario is null)
            {
                return NotFound();
            }

            NombreUsuario = usuario.NombreUsuario;
            NombreCompleto = usuario.NombreCompleto;
            Rol = usuario.Rol;
            Activo = usuario.Activo;
            InventarioIds = usuario.Inventarios.Select(i => i.Id).ToList();

            await CargarInventariosAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await CargarInventariosAsync();
                return Page();
            }

            var request = new ActualizarUsuarioRequest(
                NombreUsuario.Trim(),
                string.IsNullOrWhiteSpace(NombreCompleto) ? null : NombreCompleto.Trim(),
                Rol, Activo, InventarioIds);

            try
            {
                await _usuarioApiService.ActualizarAsync(Id, request);
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
