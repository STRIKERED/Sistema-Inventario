using Inventario.Core.Dtos;
using Inventario.Web.Services.Http;

namespace Inventario.Web.Services.Api;

public interface IProductoApiService
{
    Task<IReadOnlyList<ProductoDto>> ObtenerPorInventarioAsync(int inventarioId, CancellationToken ct = default);
    Task<ProductoDto?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<ProductoDto> CrearAsync(CrearProductoRequest request, CancellationToken ct = default);
    Task ActualizarAsync(int id, ActualizarProductoRequest request, CancellationToken ct = default);
}

public class ProductoApiService : ApiServiceBase, IProductoApiService
{
    public ProductoApiService(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<IReadOnlyList<ProductoDto>> ObtenerPorInventarioAsync(int inventarioId, CancellationToken ct = default) =>
        await GetAsync<List<ProductoDto>>($"api/productos/inventario/{inventarioId}", ct);

    public async Task<ProductoDto?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
    {
        try
        {
            return await GetAsync<ProductoDto>($"api/productos/{id}", ct);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }

    public Task<ProductoDto> CrearAsync(CrearProductoRequest request, CancellationToken ct = default) =>
        PostAsync<CrearProductoRequest, ProductoDto>("api/productos", request, ct);

    public Task ActualizarAsync(int id, ActualizarProductoRequest request, CancellationToken ct = default) =>
        PutAsync($"api/productos/{id}", request, ct);
}
