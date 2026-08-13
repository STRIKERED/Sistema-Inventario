using System.Net.Http.Headers;
using Inventario.Web.Services.Sesion;

namespace Inventario.Web.Services.Http;

/// <summary>Adjunta el JWT de la sesión actual (guardado en la cookie, ver ICurrentSessionAccessor)
/// como Authorization: Bearer a cada llamada saliente a Inventario.Api. Si no hay sesión (p. ej. el
/// propio login) simplemente no agrega el header.</summary>
public class AuthHeaderHandler : DelegatingHandler
{
    private readonly ICurrentSessionAccessor _sesionActual;

    public AuthHeaderHandler(ICurrentSessionAccessor sesionActual)
    {
        _sesionActual = sesionActual;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_sesionActual.Token is { Length: > 0 } token)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
