using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Inventario.Core.Dtos;
using Inventario.Core.Enums;
using Inventario.Desktop.Services.Api;
using Inventario.Desktop.Services.Sesion;

namespace Inventario.Desktop.ViewModels;

public partial class UsuariosViewModel : BaseViewModel
{
    private readonly IUsuarioApiService _usuarioApiService;
    private readonly ISucursalApiService _sucursalApiService;

    public UsuariosViewModel(IUsuarioApiService usuarioApiService, ISucursalApiService sucursalApiService, ISessionService sessionService)
        : base(sessionService)
    {
        _usuarioApiService = usuarioApiService;
        _sucursalApiService = sucursalApiService;
    }

    public ObservableCollection<UsuarioDto> Usuarios { get; } = new();
    public ObservableCollection<SucursalDto> Sucursales { get; } = new();

    // La lista de la que habla el pedido: todos los valores del enum, para un Picker de selección única.
    public IReadOnlyList<RolUsuario> Roles { get; } = Enum.GetValues<RolUsuario>();

    [ObservableProperty]
    private bool mostrarFormularioNuevoUsuario;

    [ObservableProperty]
    private string nombreUsuario = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string? nombreCompleto;

    [ObservableProperty]
    private RolUsuario rolSeleccionado = RolUsuario.Vendedor;

    // Nullable a propósito: Administrador puede no tener una sucursal fija (ver Usuario.SucursalId).
    [ObservableProperty]
    private SucursalDto? sucursalSeleccionada;

    [RelayCommand]
    private async Task CargarAsync()
    {
        await EjecutarAsync(async () =>
        {
            Usuarios.Clear();
            foreach (var usuario in await _usuarioApiService.ObtenerTodosAsync())
            {
                Usuarios.Add(usuario);
            }

            if (Sucursales.Count == 0)
            {
                foreach (var sucursal in await _sucursalApiService.ObtenerTodasAsync())
                {
                    Sucursales.Add(sucursal);
                }
            }
        });
    }

    [RelayCommand]
    private void MostrarFormulario() => MostrarFormularioNuevoUsuario = true;

    [RelayCommand]
    private void CancelarFormulario()
    {
        MostrarFormularioNuevoUsuario = false;
        NombreUsuario = string.Empty;
        Password = string.Empty;
        NombreCompleto = null;
        RolSeleccionado = RolUsuario.Vendedor;
        SucursalSeleccionada = null;
    }

    [RelayCommand]
    private async Task GuardarUsuarioAsync()
    {
        if (string.IsNullOrWhiteSpace(NombreUsuario) || string.IsNullOrWhiteSpace(Password))
        {
            MensajeError = "Usuario y contraseña son obligatorios.";
            return;
        }

        if (Password.Length < 6)
        {
            MensajeError = "La contraseña debe tener al menos 6 caracteres.";
            return;
        }

        await EjecutarAsync(async () =>
        {
            var request = new CrearUsuarioRequest(
                NombreUsuario.Trim(),
                Password,
                string.IsNullOrWhiteSpace(NombreCompleto) ? null : NombreCompleto.Trim(),
                RolSeleccionado,
                SucursalSeleccionada?.Id);

            var creado = await _usuarioApiService.CrearAsync(request);
            Usuarios.Add(creado);
            CancelarFormularioCommand.Execute(null);
        });
    }

    // Alta/baja rápida desde la lista: reutiliza ActualizarUsuarioRequest con los mismos datos y el
    // Activo invertido, igual que haría un formulario de edición completo pero sin uno dedicado.
    [RelayCommand]
    private async Task CambiarActivoAsync(UsuarioDto? usuario)
    {
        if (usuario is null)
        {
            return;
        }

        await EjecutarAsync(async () =>
        {
            var request = new ActualizarUsuarioRequest(
                usuario.NombreUsuario,
                usuario.NombreCompleto,
                usuario.Rol,
                !usuario.Activo,
                usuario.SucursalId);

            await _usuarioApiService.ActualizarAsync(usuario.Id, request);

            var indice = Usuarios.IndexOf(usuario);
            if (indice >= 0)
            {
                Usuarios[indice] = usuario with { Activo = !usuario.Activo };
            }
        });
    }
}
