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

    public LoginViewModel(IAuthApiService authApiService, ISessionService sessionService)
        : base(sessionService)
    {
        _authApiService = authApiService;
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

    // Se usa cuando LoginResponse.Inventarios trae más de un elemento: hay que elegir con cuál se
    // va a operar antes de entrar al punto de venta (si trae exactamente uno, se autoselecciona).
    [ObservableProperty]
    private bool requiereSeleccionInventario;

    [ObservableProperty]
    private InventarioDto? inventarioSeleccionado;

    public ObservableCollection<InventarioDto> Inventarios { get; } = new();

    // MostrarLoginNormal no es [ObservableProperty] porque depende de otras dos propiedades: se
    // renotifica a mano desde los partial OnXxxChanged de esas dos.
    public bool MostrarLoginNormal => !RequiereConfiguracionInicial && !RequiereSeleccionInventario;

    partial void OnRequiereConfiguracionInicialChanged(bool value) => OnPropertyChanged(nameof(MostrarLoginNormal));

    partial void OnRequiereSeleccionInventarioChanged(bool value) => OnPropertyChanged(nameof(MostrarLoginNormal));

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
    private async Task ConfirmarInventarioAsync()
    {
        if (InventarioSeleccionado is null)
        {
            MensajeError = "Selecciona un inventario para continuar.";
            return;
        }

        await EjecutarAsync(async () =>
        {
            await SessionService.FijarInventarioOperativoAsync(InventarioSeleccionado.Id);
            await IrAlPuntoDeVentaAsync();
        });
    }

    private async Task ManejarInicioSesionExitosoAsync(LoginResponse respuesta)
    {
        await SessionService.IniciarSesionAsync(respuesta);
        Password = string.Empty;
        ConfirmarPassword = string.Empty;
        RequiereConfiguracionInicial = false;

        if (respuesta.Inventarios.Count == 1)
        {
            await SessionService.FijarInventarioOperativoAsync(respuesta.Inventarios[0].Id);
            await IrAlPuntoDeVentaAsync();
            return;
        }

        if (respuesta.Inventarios.Count == 0)
        {
            MensajeError = "Tu usuario no tiene ningún inventario asignado. Pide a un Administrador que te asigne uno.";
            return;
        }

        Inventarios.Clear();
        foreach (var inventario in respuesta.Inventarios)
        {
            Inventarios.Add(inventario);
        }

        RequiereSeleccionInventario = true;
    }

    private static Task IrAlPuntoDeVentaAsync() => Shell.Current.GoToAsync("//venta");
}
