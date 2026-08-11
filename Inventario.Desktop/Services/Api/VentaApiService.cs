using Inventario.Core.Dtos;
using Inventario.Desktop.Services.Http;

namespace Inventario.Desktop.Services.Api;

public interface IVentaApiService
{
    Task<VentaDto> CrearAsync(CrearVentaRequest request, CancellationToken ct = default);
    Task<VentaDto?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<VentaDto>> ObtenerPorCorteDeCajaAsync(int corteDeCajaId, CancellationToken ct = default);
    Task<byte[]> ObtenerTicketAsync(int id, CancellationToken ct = default);
    Task ImprimirAsync(int id, string impresora, CancellationToken ct = default);
}

public class VentaApiService : ApiServiceBase, IVentaApiService
{
    public VentaApiService(HttpClient httpClient) : base(httpClient)
    {
    }

    public Task<VentaDto> CrearAsync(CrearVentaRequest request, CancellationToken ct = default) =>
        PostAsync<CrearVentaRequest, VentaDto>("api/ventas", request, ct);

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

    public Task<byte[]> ObtenerTicketAsync(int id, CancellationToken ct = default) =>
        GetBytesAsync($"api/ventas/{id}/ticket", ct);

    public Task ImprimirAsync(int id, string impresora, CancellationToken ct = default) =>
        PostAsync($"api/ventas/{id}/imprimir?impresora={Uri.EscapeDataString(impresora)}", ct);
}
