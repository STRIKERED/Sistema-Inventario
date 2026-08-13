using Inventario.Core.Dtos;
using Inventario.Web.Services.Http;

namespace Inventario.Web.Services.Api;

// La Api restringe estos endpoints a Administrador/Gerente (consulta) y Administrador (alta/edición) —
// ver [Authorize] en Inventario.Api/Controllers/UsuariosController. No se duplica esa regla aquí: si
// alguien sin permiso llega a golpear el endpoint, la Api responde 403 y la página lo muestra como
// cualquier otro ApiException.
public interface IUsuarioApiService
{
    Task<IReadOnlyList<UsuarioDto>> ObtenerTodosAsync(CancellationToken ct = default);
    Task<UsuarioDto?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<UsuarioDto> CrearAsync(CrearUsuarioRequest request, CancellationToken ct = default);
    Task ActualizarAsync(int id, ActualizarUsuarioRequest request, CancellationToken ct = default);
}

public class UsuarioApiService : ApiServiceBase, IUsuarioApiService
{
    public UsuarioApiService(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<IReadOnlyList<UsuarioDto>> ObtenerTodosAsync(CancellationToken ct = default) =>
        await GetAsync<List<UsuarioDto>>("api/usuarios", ct);

    public async Task<UsuarioDto?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
    {
        try
        {
            return await GetAsync<UsuarioDto>($"api/usuarios/{id}", ct);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }

    public Task<UsuarioDto> CrearAsync(CrearUsuarioRequest request, CancellationToken ct = default) =>
        PostAsync<CrearUsuarioRequest, UsuarioDto>("api/usuarios", request, ct);

    public Task ActualizarAsync(int id, ActualizarUsuarioRequest request, CancellationToken ct = default) =>
        PutAsync($"api/usuarios/{id}", request, ct);
}
