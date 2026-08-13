using Inventario.Core.Dtos;
using Inventario.Web.Services.Http;

namespace Inventario.Web.Services.Api;

public interface IVentaApiService
{
    Task<VentaDto?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<VentaDto>> ObtenerPorCorteDeCajaAsync(int corteDeCajaId, CancellationToken ct = default);

    /// <summary>Sin desde/hasta trae las de hoy. <paramref name="cancelada"/> en false (default) trae
    /// el historial normal; en true, solo las canceladas.</summary>
    Task<IReadOnlyList<VentaDto>> ObtenerPorInventarioAsync(
        int inventarioId, DateTime? desde = null, DateTime? hasta = null, bool cancelada = false, CancellationToken ct = default);

    Task<byte[]> ObtenerTicketAsync(int id, CancellationToken ct = default);
}

public class VentaApiService : ApiServiceBase, IVentaApiService
{
    public VentaApiService(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<VentaDto?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
    {
        try
        {
            return await GetAsync<VentaDto>($"api/ventas/{id}", ct);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<VentaDto>> ObtenerPorCorteDeCajaAsync(int corteDeCajaId, CancellationToken ct = default) =>
        await GetAsync<List<VentaDto>>($"api/ventas/corte/{corteDeCajaId}", ct);

    public async Task<IReadOnlyList<VentaDto>> ObtenerPorInventarioAsync(
        int inventarioId, DateTime? desde = null, DateTime? hasta = null, bool cancelada = false, CancellationToken ct = default)
    {
        var query = new List<string> { $"cancelada={cancelada}" };
        if (desde is not null)
        {
            query.Add($"desde={desde:yyyy-MM-dd}");
        }

        if (hasta is not null)
        {
            query.Add($"hasta={hasta:yyyy-MM-dd}");
        }

        return await GetAsync<List<VentaDto>>($"api/ventas/inventario/{inventarioId}?{string.Join("&", query)}", ct);
    }

    public Task<byte[]> ObtenerTicketAsync(int id, CancellationToken ct = default) =>
        GetBytesAsync($"api/ventas/{id}/ticket", ct);
}
