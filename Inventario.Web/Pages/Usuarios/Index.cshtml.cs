using Inventario.Core.Dtos;
using Inventario.Web.Services.Api;
using Inventario.Web.Services.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Usuarios
{
    /// <summary>Lista de usuarios con su rol y a qué Inventarios tiene acceso cada uno. La Api permite
    /// consultar a Administrador y Gerente, pero crear/editar solo a Administrador (por eso el botón
    /// "Nuevo usuario"/"Editar" se oculta para Gerente en la vista, aunque aquí no se restringe la
    /// página completa: Gerente sí puede ver quién tiene acceso a qué).</summary>
    [Authorize(Roles = "Administrador,Gerente")]
    public class IndexModel : PageModel
    {
        private readonly IUsuarioApiService _usuarioApiService;

        public IndexModel(IUsuarioApiService usuarioApiService)
        {
            _usuarioApiService = usuarioApiService;
        }

        [BindProperty]
        public int UsuarioId { get; set; }

        public IReadOnlyList<UsuarioDto> Usuarios { get; private set; } = [];
        public string? ErrorMensaje { get; private set; }

        public async Task OnGetAsync() => await CargarAsync();

        // Alta/baja rápida desde la lista, mismo patrón que Inventarios/Index. Solo Administrador.
        // Nota: [Authorize] no se puede aplicar a nivel de handler en Razor Pages (solo a nivel de
        // página), así que el rol se revisa a mano aquí — la Api igual rechazaría el PUT con 403 si
        // un Gerente lo intentara de todas formas, esto es solo para no depender de eso.
        public async Task<IActionResult> OnPostCambiarActivoAsync()
        {
            if (!User.IsInRole("Administrador"))
            {
                return Forbid();
            }

            try
            {
                var actual = await _usuarioApiService.ObtenerPorIdAsync(UsuarioId);
                if (actual is null)
                {
                    return NotFound();
                }

                var request = new ActualizarUsuarioRequest(
                    actual.NombreUsuario, actual.NombreCompleto, actual.Rol, !actual.Activo,
                    actual.Inventarios.Select(i => i.Id).ToList());

                await _usuarioApiService.ActualizarAsync(UsuarioId, request);
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
                Usuarios = await _usuarioApiService.ObtenerTodosAsync();
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                ErrorMensaje = ex.Message;
            }
        }
    }
}
