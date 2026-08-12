using Inventario.Core.Dtos;
using Inventario.Desktop.Services.Http;

namespace Inventario.Desktop.Services.Api;

public interface IProductoApiService
{
    Task<IReadOnlyList<ProductoDto>> ObtenerPorInventarioAsync(int inventarioId, CancellationToken ct = default);
    Task<ProductoDto?> ObtenerPorCodigoBarrasAsync(string codigoBarras, int inventarioId, CancellationToken ct = default);
    Task<ProductoDto?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
}

public class ProductoApiService : ApiServiceBase, IProductoApiService
{
    public ProductoApiService(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<IReadOnlyList<ProductoDto>> ObtenerPorInventarioAsync(int inventarioId, CancellationToken ct = default) =>
        await GetAsync<List<ProductoDto>>($"api/productos/inventario/{inventarioId}", ct);

    public Task<ProductoDto?> ObtenerPorCodigoBarrasAsync(string codigoBarras, int inventarioId, CancellationToken ct = default) =>
        ObtenerOpcionalAsync($"api/productos/codigo-barras/{Uri.EscapeDataString(codigoBarras)}?inventarioId={inventarioId}", ct);

    public Task<ProductoDto?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        ObtenerOpcionalAsync($"api/productos/{id}", ct);

    // Un código no encontrado es un resultado esperado durante un escaneo (código mal leído, producto
    // no dado de alta, etc.), no una condición excepcional: se traduce a null en vez de propagar la 404.
    private async Task<ProductoDto?> ObtenerOpcionalAsync(string url, CancellationToken ct)
    {
        try
        {
            return await GetAsync<ProductoDto>(url, ct);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }
}
