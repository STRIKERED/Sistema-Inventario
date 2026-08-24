using System.ComponentModel.DataAnnotations;
using Inventario.Core.Dtos;
using Inventario.Web.Services.Api;
using Inventario.Web.Services.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Configuracion
{
    /// <summary>Edita la Sucursal de esta instalación. Cada base de datos local tiene exactamente
    /// una (arquitectura "una sucursal, una base"), así que no hace falta elegir cuál.</summary>
    [Authorize(Roles = "Administrador")]
    public class TiendaModel : PageModel
    {
        private readonly ISucursalApiService _sucursalApiService;

        public TiendaModel(ISucursalApiService sucursalApiService)
        {
            _sucursalApiService = sucursalApiService;
        }

        [BindProperty]
        public int Id { get; set; }

        [BindProperty, Required(ErrorMessage = "El nombre es obligatorio."), StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [BindProperty, StringLength(300)]
        public string? Direccion { get; set; }

        public string? ErrorMensaje { get; private set; }
        public string? MensajeExito { get; private set; }

        public async Task OnGetAsync()
        {
            try
            {
                var sucursal = (await _sucursalApiService.ObtenerTodasAsync()).FirstOrDefault();
                if (sucursal is null)
                {
                    ErrorMensaje = "No hay ninguna sucursal registrada.";
                    return;
                }

                Id = sucursal.Id;
                Nombre = sucursal.Nombre;
                Direccion = sucursal.Direccion;
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                ErrorMensaje = ex.Message;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                await _sucursalApiService.ActualizarAsync(Id, new SucursalRequest(
                    Nombre.Trim(), string.IsNullOrWhiteSpace(Direccion) ? null : Direccion.Trim()));
                MensajeExito = "Datos guardados.";
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                ErrorMensaje = ex.Message;
            }

            return Page();
        }
    }
}
