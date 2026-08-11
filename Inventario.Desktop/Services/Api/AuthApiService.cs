using Inventario.Core.Dtos;
using Inventario.Desktop.Services.Http;

namespace Inventario.Desktop.Services.Api;

public interface IAuthApiService
{
    Task<LoginResponse> LoginAsync(string nombreUsuario, string password, CancellationToken ct = default);

    /// <summary>Consulta si ya existe algún usuario, para decidir si el Login muestra el formulario
    /// normal o el de "crear el primer Administrador".</summary>
    Task<EstadoSistemaResponse> ObtenerEstadoAsync(CancellationToken ct = default);

    /// <summary>Solo funciona si todavía no hay ningún usuario en el sistema; ver AuthController.RegistroInicial.</summary>
    Task<LoginResponse> RegistrarUsuarioInicialAsync(RegistrarUsuarioInicialRequest request, CancellationToken ct = default);
}

public class AuthApiService : ApiServiceBase, IAuthApiService
{
    public AuthApiService(HttpClient httpClient) : base(httpClient)
    {
    }

    public Task<LoginResponse> LoginAsync(string nombreUsuario, string password, CancellationToken ct = default) =>
        PostAsync<LoginRequest, LoginResponse>("api/auth/login", new LoginRequest(nombreUsuario, password), ct);

    public Task<EstadoSistemaResponse> ObtenerEstadoAsync(CancellationToken ct = default) =>
        GetAsync<EstadoSistemaResponse>("api/auth/estado", ct);

    public Task<LoginResponse> RegistrarUsuarioInicialAsync(RegistrarUsuarioInicialRequest request, CancellationToken ct = default) =>
        PostAsync<RegistrarUsuarioInicialRequest, LoginResponse>("api/auth/registro-inicial", request, ct);
}
