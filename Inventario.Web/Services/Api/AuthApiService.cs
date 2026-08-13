using Inventario.Core.Dtos;
using Inventario.Web.Services.Http;

namespace Inventario.Web.Services.Api;

public interface IAuthApiService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
}

/// <summary>Sin AuthHeaderHandler: login se llama antes de que exista una sesión/JWT que adjuntar.</summary>
public class AuthApiService : ApiServiceBase, IAuthApiService
{
    public AuthApiService(HttpClient httpClient) : base(httpClient)
    {
    }

    public Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default) =>
        PostAsync<LoginRequest, LoginResponse>("api/auth/login", request, ct);
}
