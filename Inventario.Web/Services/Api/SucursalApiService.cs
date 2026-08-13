using Inventario.Core.Dtos;
using Inventario.Web.Services.Http;

namespace Inventario.Web.Services.Api;

public interface ISucursalApiService
{
    Task<IReadOnlyList<SucursalDto>> ObtenerTodasAsync(CancellationToken ct = default);
    Task ActualizarAsync(int id, SucursalRequest request, CancellationToken ct = default);
}

public class SucursalApiService : ApiServiceBase, ISucursalApiService
{
    public SucursalApiService(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<IReadOnlyList<SucursalDto>> ObtenerTodasAsync(CancellationToken ct = default) =>
        await GetAsync<List<SucursalDto>>("api/sucursales", ct);

    public Task ActualizarAsync(int id, SucursalRequest request, CancellationToken ct = default) =>
        PutAsync($"api/sucursales/{id}", request, ct);
}
