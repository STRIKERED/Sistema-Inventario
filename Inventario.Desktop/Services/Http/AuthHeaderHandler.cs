using System.Net.Http.Headers;
using Inventario.Desktop.Services.Sesion;

namespace Inventario.Desktop.Services.Http;

/// <summary>
/// DelegatingHandler registrado en cada HttpClient con nombre "InventarioApi": agrega el JWT de la
/// sesión activa como header Authorization en cada request saliente, sin que cada ApiService tenga
/// que preocuparse por ello.
/// </summary>
public class AuthHeaderHandler : DelegatingHandler
{
    private readonly ISessionService _sessionService;

    public AuthHeaderHandler(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(_sessionService.Token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _sessionService.Token);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
