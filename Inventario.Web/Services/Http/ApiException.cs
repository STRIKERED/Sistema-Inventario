namespace Inventario.Web.Services.Http;

/// <summary>Error al llamar a Inventario.Api: StatusCode + un mensaje ya legible (extraído del cuerpo
/// de la respuesta). Mismo patrón que Inventario.Desktop.Services.Http.ApiException.</summary>
public class ApiException : Exception
{
    public int StatusCode { get; }

    public ApiException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }
}
