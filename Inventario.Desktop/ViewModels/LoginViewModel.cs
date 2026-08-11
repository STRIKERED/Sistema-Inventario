using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Inventario.Core.Dtos;
using Inventario.Desktop.Services.Api;
using Inventario.Desktop.Services.Sesion;

namespace Inventario.Desktop.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthApiService _authApiService;
    private readonly ISucursalApiService _sucursalApiService;

    public LoginViewModel(IAuthApiService authApiService, ISucursalApiService sucursalApiService, ISessionService sessionService)
        : base(sessionService)
    {
        _authApiService = authApiService;
        _sucursalApiService = sucursalApiService;
    }

    [ObservableProperty]
    private string nombreUsuario = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    // Solo se pide en el formulario de "crear el primer Administrador", para detectar errores de tecleo.
    [ObservableProperty]
    private string confirmarPassword = string.Empty;

    [ObservableProperty]
    private string? nombreCompleto;

    // Empieza en false (se asume login normal) y CargarEstadoCommand lo corrige apenas la pantalla
    // aparece; así, si la consulta a /api/auth/estado falla por algo de red, el usuario igual puede
    // intentar loguearse en vez de quedarse varado sin ningún formulario visible.
    [ObservableProperty]
    private bool requiereConfiguracionInicial;

    // Solo se usa cuando el usuario (típicamente Administrador) no trae una sucursal fija en el JWT:
    // hay que elegir con cuál va a operar antes de entrar al punto de venta.
    [ObservableProperty]
    private bool requiereSeleccionSucursal;

    [ObservableProperty]
    private SucursalDto? sucursalSeleccionada;

    public ObservableCollection<SucursalDto> Sucursales { get; } = new();

    // MostrarLoginNormal no es [ObservableProperty] porque depende de otras dos propiedades: se
    // renotifica a mano desde los partial OnXxxChanged de esas dos.
    public bool MostrarLoginNormal => !RequiereConfiguracionInicial && !RequiereSeleccionSucursal;

    partial void OnRequiereConfiguracionInicialChanged(bool value) => OnPropertyChanged(nameof(MostrarLoginNormal));

    partial void OnRequiereSeleccionSucursalChanged(bool value) => OnPropertyChanged(nameof(MostrarLoginNormal));

    [RelayCommand]
    private async Task CargarEstadoAsync()
    {
        await EjecutarAsync(async () =>
        {
            var estado = await _authApiService.ObtenerEstadoAsync();
            RequiereConfiguracionInicial = !estado.HayUsuarios;
        });
    }

    [RelayCommand]
    private async Task IniciarSesionAsync()
    {
        await EjecutarAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(NombreUsuario) || string.IsNullOrWhiteSpace(Password))
            {
                MensajeError = "Ingresa usuario y contraseña.";
                return;
            }

            var respuesta = await _authApiService.LoginAsync(NombreUsuario.Trim(), Password);
            await ManejarInicioSesionExitosoAsync(respuesta);
        });
    }

    [RelayCommand]
    private async Task CrearAdministradorInicialAsync()
    {
        if (string.IsNullOrWhiteSpace(NombreUsuario) || string.IsNullOrWhiteSpace(Password))
        {
            MensajeError = "Ingresa usuario y contraseña.";
            return;
        }

        if (Password.Length < 6)
        {
            MensajeError = "La contraseña debe tener al menos 6 caracteres.";
            return;
        }

        if (Password != ConfirmarPassword)
        {
            MensajeError = "Las contraseñas no coinciden.";
            return;
        }

        await EjecutarAsync(async () =>
        {
            var request = new RegistrarUsuarioInicialRequest(
                NombreUsuario.Trim(),
                Password,
                string.IsNullOrWhiteSpace(NombreCompleto) ? null : NombreCompleto.Trim());

            var respuesta = await _authApiService.RegistrarUsuarioInicialAsync(request);
            await ManejarInicioSesionExitosoAsync(respuesta);
        });
    }

    [RelayCommand]
    private async Task ConfirmarSucursalAsync()
    {
        if (SucursalSeleccionada is null)
        {
            MensajeError = "Selecciona una sucursal para continuar.";
            return;
        }

        await EjecutarAsync(async () =>
        {
            await SessionService.FijarSucursalOperativaAsync(SucursalSeleccionada.Id);
            await IrAlPuntoDeVentaAsync();
        });
    }

    private async Task ManejarInicioSesionExitosoAsync(LoginResponse respuesta)
    {
        await SessionService.IniciarSesionAsync(respuesta);
        Password = string.Empty;
        ConfirmarPassword = string.Empty;
        RequiereConfiguracionInicial = false;

        if (respuesta.SucursalId is not null)
        {
            await IrAlPuntoDeVentaAsync();
            return;
        }

        Sucursales.Clear();
        foreach (var sucursal in await _sucursalApiService.ObtenerTodasAsync())
        {
            Sucursales.Add(sucursal);
        }

        RequiereSeleccionSucursal = true;
    }

    private static Task IrAlPuntoDeVentaAsync() => Shell.Current.GoToAsync("//venta");
}
