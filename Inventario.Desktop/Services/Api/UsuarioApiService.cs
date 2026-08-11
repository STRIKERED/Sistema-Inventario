using Inventario.Core.Dtos;
using Inventario.Desktop.Services.Http;

namespace Inventario.Desktop.Services.Api;

public interface IUsuarioApiService
{
    Task<IReadOnlyList<UsuarioDto>> ObtenerTodosAsync(CancellationToken ct = default);
    Task<UsuarioDto> CrearAsync(CrearUsuarioRequest request, CancellationToken ct = default);
    Task ActualizarAsync(int id, ActualizarUsuarioRequest request, CancellationToken ct = default);
}

// Nota: el backend restringe estos endpoints a rol Administrador (creación/edición) y
// Administrador/Gerente (consulta) — ver [Authorize] en Inventario.Api/Controllers/UsuariosController.
// Aquí no se duplica esa regla; si alguien sin permiso llega a golpear el endpoint, la API responde
// 403 y BaseViewModel.EjecutarAsync lo muestra como MensajeError igual que cualquier otro ApiException.
public class UsuarioApiService : ApiServiceBase, IUsuarioApiService
{
    public UsuarioApiService(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<IReadOnlyList<UsuarioDto>> ObtenerTodosAsync(CancellationToken ct = default) =>
        await GetAsync<List<UsuarioDto>>("api/usuarios", ct);

    public Task<UsuarioDto> CrearAsync(CrearUsuarioRequest request, CancellationToken ct = default) =>
        PostAsync<CrearUsuarioRequest, UsuarioDto>("api/usuarios", request, ct);

    public Task ActualizarAsync(int id, ActualizarUsuarioRequest request, CancellationToken ct = default) =>
        PutAsync($"api/usuarios/{id}", request, ct);
}
