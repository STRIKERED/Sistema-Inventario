using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Inventario.Core.Dtos;
using Inventario.Core.Enums;
using Inventario.Desktop.Models;
using Inventario.Desktop.Services.Api;
using Inventario.Desktop.Services.Sesion;

namespace Inventario.Desktop.ViewModels;

public partial class UsuariosViewModel : BaseViewModel
{
    private readonly IUsuarioApiService _usuarioApiService;
    private readonly IInventarioApiService _inventarioApiService;

    public UsuariosViewModel(IUsuarioApiService usuarioApiService, IInventarioApiService inventarioApiService, ISessionService sessionService)
        : base(sessionService)
    {
        _usuarioApiService = usuarioApiService;
        _inventarioApiService = inventarioApiService;
    }

    public ObservableCollection<UsuarioDto> Usuarios { get; } = new();

    // Un checkbox por Inventario disponible; GuardarUsuarioAsync junta los marcados en InventarioIds.
    public ObservableCollection<InventarioSeleccionable> InventariosDisponibles { get; } = new();

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

            if (InventariosDisponibles.Count == 0)
            {
                foreach (var inventario in await _inventarioApiService.ObtenerTodosAsync())
                {
                    InventariosDisponibles.Add(new InventarioSeleccionable(inventario));
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

        foreach (var inventario in InventariosDisponibles)
        {
            inventario.Seleccionado = false;
        }
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
            var inventarioIds = InventariosDisponibles.Where(i => i.Seleccionado).Select(i => i.Id).ToList();

            var request = new CrearUsuarioRequest(
                NombreUsuario.Trim(),
                Password,
                string.IsNullOrWhiteSpace(NombreCompleto) ? null : NombreCompleto.Trim(),
                RolSeleccionado,
                inventarioIds);

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
                usuario.Inventarios.Select(i => i.Id).ToList());

            await _usuarioApiService.ActualizarAsync(usuario.Id, request);

            var indice = Usuarios.IndexOf(usuario);
            if (indice >= 0)
            {
                Usuarios[indice] = usuario with { Activo = !usuario.Activo };
            }
        });
    }
}
