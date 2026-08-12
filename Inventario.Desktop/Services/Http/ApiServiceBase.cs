using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Inventario.Desktop.Services.Http;

/// <summary>
/// Base común para los servicios Api.*: centraliza serialización (enums como texto, igual que en
/// Inventario.Api/Program.cs), armado de requests y traducción de respuestas no exitosas a
/// <see cref="ApiException"/> con un mensaje legible (soporta tanto ProblemDetails del
/// ExceptionHandlingMiddleware como los BadRequest/Unauthorized("texto plano") de los controllers).
/// </summary>
public abstract class ApiServiceBase
{
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected readonly HttpClient Http;

    protected ApiServiceBase(HttpClient httpClient)
    {
        Http = httpClient;
    }

    protected Task<T> GetAsync<T>(string url, CancellationToken ct = default) =>
        EnviarAsync<T>(new HttpRequestMessage(HttpMethod.Get, url), ct);

    protected Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest body, CancellationToken ct = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(body, options: JsonOptions) };
        return EnviarAsync<TResponse>(request, ct);
    }

    protected Task PostAsync<TRequest>(string url, TRequest body, CancellationToken ct = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(body, options: JsonOptions) };
        return EnviarSinRespuestaAsync(request, ct);
    }

    protected Task<TResponse> PutAsync<TRequest, TResponse>(string url, TRequest body, CancellationToken ct = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, url) { Content = JsonContent.Create(body, options: JsonOptions) };
        return EnviarAsync<TResponse>(request, ct);
    }

    protected Task PutAsync<TRequest>(string url, TRequest body, CancellationToken ct = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, url) { Content = JsonContent.Create(body, options: JsonOptions) };
        return EnviarSinRespuestaAsync(request, ct);
    }

    /// <summary>POST sin cuerpo (p. ej. /ventas/{id}/imprimir?impresora=...).</summary>
    protected Task PostAsync(string url, CancellationToken ct = default) =>
        EnviarSinRespuestaAsync(new HttpRequestMessage(HttpMethod.Post, url), ct);

    protected async Task<byte[]> GetBytesAsync(string url, CancellationToken ct = default)
    {
        using var response = await Http.GetAsync(url, ct);
        await AsegurarExitoAsync(response, ct);
        return await response.Content.ReadAsByteArrayAsync(ct);
    }

    /// <summary>POST multipart/form-data (p. ej. subir un archivo). No espera un cuerpo de respuesta JSON.</summary>
    protected async Task PostFileAsync(string url, string nombreCampo, string nombreArchivo, Stream contenido, CancellationToken ct = default)
    {
        using var form = new MultipartFormDataContent();
        using var streamContent = new StreamContent(contenido);
        form.Add(streamContent, nombreCampo, nombreArchivo);

        using var response = await Http.PostAsync(url, form, ct);
        await AsegurarExitoAsync(response, ct);
    }

    private async Task<T> EnviarAsync<T>(HttpRequestMessage request, CancellationToken ct)
    {
        using var response = await Http.SendAsync(request, ct);
        await AsegurarExitoAsync(response, ct);
        var resultado = await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct);
        return resultado!;
    }

    private async Task EnviarSinRespuestaAsync(HttpRequestMessage request, CancellationToken ct)
    {
        using var response = await Http.SendAsync(request, ct);
        await AsegurarExitoAsync(response, ct);
    }

    private static async Task AsegurarExitoAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var texto = await response.Content.ReadAsStringAsync(ct);
        var mensaje = texto;

        if (!string.IsNullOrWhiteSpace(texto))
        {
            try
            {
                using var documento = JsonDocument.Parse(texto);
                mensaje = documento.RootElement.ValueKind switch
                {
                    // BadRequest("texto")/Unauthorized("texto") de los controllers serializan un string JSON suelto.
                    JsonValueKind.String => documento.RootElement.GetString(),
                    // ProblemDetails del ExceptionHandlingMiddleware (o el 400 automático de [ApiController]).
                    JsonValueKind.Object => ExtraerMensajeDeProblemDetails(documento.RootElement) ?? texto,
                    _ => texto
                };
            }
            catch (JsonException)
            {
                // El cuerpo no era JSON (poco común, pero no debe tumbar la app): se usa tal cual.
            }
        }

        if (string.IsNullOrWhiteSpace(mensaje))
        {
            mensaje = $"Error {(int)response.StatusCode} al comunicarse con el servidor.";
        }

        throw new ApiException((int)response.StatusCode, mensaje);
    }

    private static string? ExtraerMensajeDeProblemDetails(JsonElement raiz)
    {
        if (raiz.TryGetProperty("detail", out var detail) && detail.ValueKind == JsonValueKind.String)
        {
            return detail.GetString();
        }

        if (raiz.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
        {
            // 400 automático de ModelState inválido: { errors: { "Campo": ["mensaje1", "mensaje2"] } }
            var primerCampo = errors.EnumerateObject().FirstOrDefault();
            if (primerCampo.Value.ValueKind == JsonValueKind.Array && primerCampo.Value.GetArrayLength() > 0)
            {
                return primerCampo.Value[0].GetString();
            }
        }

        if (raiz.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
        {
            return title.GetString();
        }

        return null;
    }
}
