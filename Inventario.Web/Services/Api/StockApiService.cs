using Inventario.Core.Dtos;
using Inventario.Core.Enums;
using Inventario.Web.Services.Http;

namespace Inventario.Web.Services.Api;

public interface IStockApiService
{
    Task<IReadOnlyList<MovimientoInventarioDto>> ObtenerMovimientosPorProductoAsync(int productoId, CancellationToken ct = default);

    /// <summary>Filtros todos opcionales; sin ninguno trae el historial completo del Inventario.</summary>
    Task<IReadOnlyList<MovimientoInventarioDto>> ObtenerMovimientosPorInventarioAsync(
        int inventarioId, DateTime? desde = null, DateTime? hasta = null, TipoMovimientoInventario? tipo = null, CancellationToken ct = default);

    Task<MovimientoInventarioDto> RegistrarMovimientoAsync(RegistrarMovimientoRequest request, CancellationToken ct = default);
}

public class StockApiService : ApiServiceBase, IStockApiService
{
    public StockApiService(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<IReadOnlyList<MovimientoInventarioDto>> ObtenerMovimientosPorProductoAsync(int productoId, CancellationToken ct = default) =>
        await GetAsync<List<MovimientoInventarioDto>>($"api/stock/movimientos/producto/{productoId}", ct);

    public async Task<IReadOnlyList<MovimientoInventarioDto>> ObtenerMovimientosPorInventarioAsync(
        int inventarioId, DateTime? desde = null, DateTime? hasta = null, TipoMovimientoInventario? tipo = null, CancellationToken ct = default)
    {
        var query = new List<string>();
        if (desde is not null)
        {
            query.Add($"desde={desde:yyyy-MM-dd}");
        }

        if (hasta is not null)
        {
            query.Add($"hasta={hasta:yyyy-MM-dd}");
        }

        if (tipo is not null)
        {
            query.Add($"tipo={tipo}");
        }

        var queryString = query.Count > 0 ? "?" + string.Join("&", query) : string.Empty;
        return await GetAsync<List<MovimientoInventarioDto>>($"api/stock/movimientos/inventario/{inventarioId}{queryString}", ct);
    }

    public Task<MovimientoInventarioDto> RegistrarMovimientoAsync(RegistrarMovimientoRequest request, CancellationToken ct = default) =>
        PostAsync<RegistrarMovimientoRequest, MovimientoInventarioDto>("api/stock/movimientos", request, ct);
}
