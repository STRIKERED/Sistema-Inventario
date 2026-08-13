using Inventario.Core.Dtos;
using Inventario.Web.Services.Http;

namespace Inventario.Web.Services.Api;

/// <summary>Agrupa Cajas + Cortes de Caja: en la app siempre se usan juntos (elegir caja -> ver/abrir/
/// cerrar su corte, o revisar el historial). Mismo agrupamiento que
/// Inventario.Desktop.Services.Api.CajaApiService.</summary>
public interface ICajaApiService
{
    Task<IReadOnlyList<CajaDto>> ObtenerPorInventarioAsync(int inventarioId, CancellationToken ct = default);
    Task<CajaDto> CrearAsync(CajaRequest request, CancellationToken ct = default);

    Task<CorteDeCajaDto?> ObtenerCorteAbiertoAsync(int cajaId, CancellationToken ct = default);
    Task<CorteDeCajaDto?> ObtenerCortePorIdAsync(int corteId, CancellationToken ct = default);
    Task<IReadOnlyList<CorteDeCajaDto>> ObtenerCortesPorCajaAsync(int cajaId, CancellationToken ct = default);
    Task<CorteDeCajaDto> AbrirCorteAsync(AbrirCorteRequest request, CancellationToken ct = default);
    Task<CorteDeCajaDto> CerrarCorteAsync(int corteId, CerrarCorteRequest request, CancellationToken ct = default);
}

public class CajaApiService : ApiServiceBase, ICajaApiService
{
    public CajaApiService(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<IReadOnlyList<CajaDto>> ObtenerPorInventarioAsync(int inventarioId, CancellationToken ct = default) =>
        await GetAsync<List<CajaDto>>($"api/cajas/inventario/{inventarioId}", ct);

    public Task<CajaDto> CrearAsync(CajaRequest request, CancellationToken ct = default) =>
        PostAsync<CajaRequest, CajaDto>("api/cajas", request, ct);

    public async Task<CorteDeCajaDto?> ObtenerCorteAbiertoAsync(int cajaId, CancellationToken ct = default)
    {
        try
        {
            return await GetAsync<CorteDeCajaDto>($"api/cortesdecaja/caja/{cajaId}/abierto", ct);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }

    public async Task<CorteDeCajaDto?> ObtenerCortePorIdAsync(int corteId, CancellationToken ct = default)
    {
        try
        {
            return await GetAsync<CorteDeCajaDto>($"api/cortesdecaja/{corteId}", ct);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<CorteDeCajaDto>> ObtenerCortesPorCajaAsync(int cajaId, CancellationToken ct = default) =>
        await GetAsync<List<CorteDeCajaDto>>($"api/cortesdecaja/caja/{cajaId}", ct);

    public Task<CorteDeCajaDto> AbrirCorteAsync(AbrirCorteRequest request, CancellationToken ct = default) =>
        PostAsync<AbrirCorteRequest, CorteDeCajaDto>("api/cortesdecaja/abrir", request, ct);

    public Task<CorteDeCajaDto> CerrarCorteAsync(int corteId, CerrarCorteRequest request, CancellationToken ct = default) =>
        PutAsync<CerrarCorteRequest, CorteDeCajaDto>($"api/cortesdecaja/{corteId}/cerrar", request, ct);
}
