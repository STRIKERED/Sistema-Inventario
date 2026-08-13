using Inventario.Web.Services.Sesion;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Filters;

/// <summary>
/// Filtro global: si hay sesión activa pero el usuario todavía no eligió con qué Inventario operar
/// (tiene acceso a más de uno), manda a /Auth/SeleccionarInventario antes de ejecutar cualquier otra
/// página — así ninguna página de negocio tiene que acordarse de revisar
/// ICurrentSessionAccessor.InventarioOperativoId por su cuenta. Se excluye la carpeta /Auth para no
/// generar un loop de redirects contra la propia pantalla de selección (o Login, para usuarios
/// todavía anónimos).
/// </summary>
public class RequiereInventarioSeleccionadoFilter : IAsyncPageFilter
{
    private readonly ICurrentSessionAccessor _sesionActual;

    public RequiereInventarioSeleccionadoFilter(ICurrentSessionAccessor sesionActual)
    {
        _sesionActual = sesionActual;
    }

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;

    public Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var esRutaDeAuth = context.HttpContext.Request.Path.StartsWithSegments("/Auth", StringComparison.OrdinalIgnoreCase);

        if (!esRutaDeAuth && _sesionActual.DebeSeleccionarInventario)
        {
            var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
            context.Result = new RedirectResult($"/Auth/SeleccionarInventario?returnUrl={Uri.EscapeDataString(returnUrl)}");
            return Task.CompletedTask;
        }

        return next();
    }
}
