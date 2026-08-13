using Inventario.Core.Dtos;
using Inventario.Web.Services.Http;

namespace Inventario.Web.Services.Api;

public interface ICotizacionApiService
{
    Task<CotizacionDto?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<CotizacionDto>> ObtenerPorInventarioAsync(int inventarioId, CancellationToken ct = default);
    Task<IReadOnlyList<CotizacionDto>> ObtenerVigentesAsync(int inventarioId, CancellationToken ct = default);
    Task<CotizacionDto> CrearAsync(CrearCotizacionRequest request, CancellationToken ct = default);
    Task ActualizarAsync(int id, ActualizarCotizacionRequest request, CancellationToken ct = default);
    Task<VentaDto> ConvertirAVentaAsync(int id, ConvertirAVentaRequest request, CancellationToken ct = default);
    Task<byte[]> ObtenerPdfAsync(int id, CancellationToken ct = default);
}

public class CotizacionApiService : ApiServiceBase, ICotizacionApiService
{
    public CotizacionApiService(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<CotizacionDto?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
    {
        try
        {
            return await GetAsync<CotizacionDto>($"api/cotizaciones/{id}", ct);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<CotizacionDto>> ObtenerPorInventarioAsync(int inventarioId, CancellationToken ct = default) =>
        await GetAsync<List<CotizacionDto>>($"api/cotizaciones/inventario/{inventarioId}", ct);

    public async Task<IReadOnlyList<CotizacionDto>> ObtenerVigentesAsync(int inventarioId, CancellationToken ct = default) =>
        await GetAsync<List<CotizacionDto>>($"api/cotizaciones/vigentes/{inventarioId}", ct);

    public Task<CotizacionDto> CrearAsync(CrearCotizacionRequest request, CancellationToken ct = default) =>
        PostAsync<CrearCotizacionRequest, CotizacionDto>("api/cotizaciones", request, ct);

    public Task ActualizarAsync(int id, ActualizarCotizacionRequest request, CancellationToken ct = default) =>
        PutAsync($"api/cotizaciones/{id}", request, ct);

    public Task<VentaDto> ConvertirAVentaAsync(int id, ConvertirAVentaRequest request, CancellationToken ct = default) =>
        PostAsync<ConvertirAVentaRequest, VentaDto>($"api/cotizaciones/{id}/convertir-a-venta", request, ct);

    public Task<byte[]> ObtenerPdfAsync(int id, CancellationToken ct = default) =>
        GetBytesAsync($"api/cotizaciones/{id}/pdf", ct);
}
