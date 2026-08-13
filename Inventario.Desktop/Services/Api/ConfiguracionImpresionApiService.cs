using Inventario.Core.Dtos;
using Inventario.Desktop.Services.Http;

namespace Inventario.Desktop.Services.Api;

public interface IConfiguracionImpresionApiService
{
    Task<ConfiguracionImpresionDto?> ObtenerPorInventarioAsync(int inventarioId, CancellationToken ct = default);
    Task<ConfiguracionImpresionDto> CrearAsync(ConfiguracionImpresionRequest request, CancellationToken ct = default);
    Task ActualizarAsync(int id, ConfiguracionImpresionRequest request, CancellationToken ct = default);
}

public class ConfiguracionImpresionApiService : ApiServiceBase, IConfiguracionImpresionApiService
{
    public ConfiguracionImpresionApiService(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<ConfiguracionImpresionDto?> ObtenerPorInventarioAsync(int inventarioId, CancellationToken ct = default)
    {
        try
        {
            return await GetAsync<ConfiguracionImpresionDto>($"api/configuracionesimpresion/inventario/{inventarioId}", ct);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }

    public Task<ConfiguracionImpresionDto> CrearAsync(ConfiguracionImpresionRequest request, CancellationToken ct = default) =>
        PostAsync<ConfiguracionImpresionRequest, ConfiguracionImpresionDto>("api/configuracionesimpresion", request, ct);

    public Task ActualizarAsync(int id, ConfiguracionImpresionRequest request, CancellationToken ct = default) =>
        PutAsync($"api/configuracionesimpresion/{id}", request, ct);
}
