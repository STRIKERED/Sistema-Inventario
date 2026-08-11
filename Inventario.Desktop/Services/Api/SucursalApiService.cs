using Inventario.Core.Dtos;
using Inventario.Desktop.Services.Http;

namespace Inventario.Desktop.Services.Api;

public interface ISucursalApiService
{
    Task<IReadOnlyList<SucursalDto>> ObtenerTodasAsync(CancellationToken ct = default);
}

public class SucursalApiService : ApiServiceBase, ISucursalApiService
{
    public SucursalApiService(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<IReadOnlyList<SucursalDto>> ObtenerTodasAsync(CancellationToken ct = default) =>
        await GetAsync<List<SucursalDto>>("api/sucursales", ct);
}
