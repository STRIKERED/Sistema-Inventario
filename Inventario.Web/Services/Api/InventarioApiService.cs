using Inventario.Core.Dtos;
using Inventario.Web.Services.Http;

namespace Inventario.Web.Services.Api;

public interface IInventarioApiService
{
    Task<IReadOnlyList<InventarioDto>> ObtenerTodosAsync(CancellationToken ct = default);
    Task<InventarioDto?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<InventarioDto>> ObtenerPorSucursalAsync(int sucursalId, CancellationToken ct = default);
    Task<InventarioDto> CrearAsync(InventarioRequest request, CancellationToken ct = default);
    Task ActualizarAsync(int id, InventarioRequest request, CancellationToken ct = default);
}

public class InventarioApiService : ApiServiceBase, IInventarioApiService
{
    public InventarioApiService(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<IReadOnlyList<InventarioDto>> ObtenerTodosAsync(CancellationToken ct = default) =>
        await GetAsync<List<InventarioDto>>("api/inventarios", ct);

    public async Task<InventarioDto?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
    {
        try
        {
            return await GetAsync<InventarioDto>($"api/inventarios/{id}", ct);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<InventarioDto>> ObtenerPorSucursalAsync(int sucursalId, CancellationToken ct = default) =>
        await GetAsync<List<InventarioDto>>($"api/inventarios/sucursal/{sucursalId}", ct);

    public Task<InventarioDto> CrearAsync(InventarioRequest request, CancellationToken ct = default) =>
        PostAsync<InventarioRequest, InventarioDto>("api/inventarios", request, ct);

    public Task ActualizarAsync(int id, InventarioRequest request, CancellationToken ct = default) =>
        PutAsync($"api/inventarios/{id}", request, ct);
}
