using Inventario.Web.Services.Http;
using Inventario.Web.Services.Sesion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Inventario.Web.Filters;

/// <summary>
/// Filtro global de páginas: si un handler deja escapar un <see cref="ApiException"/> con
/// StatusCode 401 (JWT vencido o inválido), cierra la sesión y redirige a Login en vez de mostrar la
/// página de error genérica. Evita que cada página tenga que acordarse de revisarlo (mismo rol que
/// BaseViewModel.EjecutarAsync en Inventario.Desktop). Otros ApiException (400/403/404/...) no se
/// tocan aquí: cada página los captura y muestra su propio mensaje, porque la respuesta apropiada
/// depende del contexto (validación de formulario vs. "no encontrado", etc.).
/// </summary>
public class ManejoErroresApiFilter : IAsyncExceptionFilter
{
    private readonly ISesionAuthService _sesionAuthService;

    public ManejoErroresApiFilter(ISesionAuthService sesionAuthService)
    {
        _sesionAuthService = sesionAuthService;
    }

    public async Task OnExceptionAsync(ExceptionContext context)
    {
        if (context.Exception is not ApiException { StatusCode: 401 })
        {
            return;
        }

        await _sesionAuthService.CerrarSesionAsync();

        var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
        context.Result = new RedirectResult($"/Auth/Login?returnUrl={Uri.EscapeDataString(returnUrl)}");
        context.ExceptionHandled = true;
    }
}
