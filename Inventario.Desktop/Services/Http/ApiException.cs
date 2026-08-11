namespace Inventario.Desktop.Services.Http;

/// <summary>Excepción lanzada por los servicios de Api.* cuando la respuesta HTTP no fue exitosa.</summary>
public class ApiException : Exception
{
    public int StatusCode { get; }

    public ApiException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }
}
