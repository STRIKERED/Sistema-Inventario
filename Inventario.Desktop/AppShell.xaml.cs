using Inventario.Core.Enums;
using Inventario.Desktop.Services.Sesion;

namespace Inventario.Desktop;

public partial class AppShell : Shell
{
    private readonly ISessionService _sessionService;

    public AppShell(ISessionService sessionService)
    {
        InitializeComponent();
        _sessionService = sessionService;

        Navigated += (_, _) => ActualizarEncabezado();
        Loaded += async (_, _) => await InicializarAsync();
    }

    private async Task InicializarAsync()
    {
        // Al abrir la app se intenta restaurar la sesión de un lanzamiento anterior (token guardado en
        // SecureStorage). Si no hay sesión válida, o el token ya venció, se cae a Login de todas formas:
        // BaseViewModel.EjecutarAsync ya redirige a "//login" en cuanto la API responda 401.
        var haySesion = await _sessionService.RestaurarSesionAsync();
        await GoToAsync(haySesion ? "//venta" : "//login");
    }

    private void ActualizarEncabezado()
    {
        EncabezadoUsuarioLabel.Text = _sessionService.HaySesionActiva
            ? _sessionService.NombreCompleto is { Length: > 0 } nombre ? nombre : _sessionService.NombreUsuario
            : "Inventario POS";

        // La API ya rechaza con 403 a quien no sea Administrador en /api/usuarios (POST/PUT), pero
        // igual se oculta la pestaña: evita que Gerente/Vendedor/Cajero lleguen a un formulario que
        // de todas formas les va a fallar al guardar.
        UsuariosFlyoutItem.IsVisible = _sessionService.Rol == RolUsuario.Administrador;
        RespaldoFlyoutItem.IsVisible = _sessionService.Rol == RolUsuario.Administrador;
    }

    private async void OnCerrarSesionClicked(object? sender, EventArgs e)
    {
        await _sessionService.CerrarSesionAsync();
        await GoToAsync("//login");
    }
}
