using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Inventario.Core.Dtos;
using Inventario.Desktop.Services.Api;
using Inventario.Desktop.Services.Sesion;

namespace Inventario.Desktop.ViewModels;

/// <summary>Alta y baja de Inventarios (p. ej. "Papelería"/"Abarrotes" dentro de una Sucursal).
/// Solo visible para Administrador (ver AppShell.xaml.cs).</summary>
public partial class InventariosViewModel : BaseViewModel
{
    private readonly IInventarioApiService _inventarioApiService;
    private readonly ISucursalApiService _sucursalApiService;

    public InventariosViewModel(IInventarioApiService inventarioApiService, ISucursalApiService sucursalApiService, ISessionService sessionService)
        : base(sessionService)
    {
        _inventarioApiService = inventarioApiService;
        _sucursalApiService = sucursalApiService;
    }

    public ObservableCollection<InventarioDto> Inventarios { get; } = new();
    public ObservableCollection<SucursalDto> Sucursales { get; } = new();

    [ObservableProperty]
    private bool mostrarFormularioNuevoInventario;

    [ObservableProperty]
    private string nombre = string.Empty;

    [ObservableProperty]
    private SucursalDto? sucursalSeleccionada;

    [RelayCommand]
    private async Task CargarAsync()
    {
        await EjecutarAsync(async () =>
        {
            Inventarios.Clear();
            foreach (var inventario in await _inventarioApiService.ObtenerTodosAsync())
            {
                Inventarios.Add(inventario);
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
    private void MostrarFormulario() => MostrarFormularioNuevoInventario = true;

    [RelayCommand]
    private void CancelarFormulario()
    {
        MostrarFormularioNuevoInventario = false;
        Nombre = string.Empty;
        SucursalSeleccionada = null;
    }

    [RelayCommand]
    private async Task GuardarInventarioAsync()
    {
        if (string.IsNullOrWhiteSpace(Nombre))
        {
            MensajeError = "El nombre es obligatorio.";
            return;
        }

        if (SucursalSeleccionada is null)
        {
            MensajeError = "Selecciona una sucursal.";
            return;
        }

        await EjecutarAsync(async () =>
        {
            var request = new InventarioRequest(Nombre.Trim(), SucursalSeleccionada.Id, true);
            var creado = await _inventarioApiService.CrearAsync(request);
            Inventarios.Add(creado);
            CancelarFormularioCommand.Execute(null);
        });
    }

    // Alta/baja rápida desde la lista, mismo patrón que UsuariosViewModel.CambiarActivoAsync.
    [RelayCommand]
    private async Task CambiarActivoAsync(InventarioDto? inventario)
    {
        if (inventario is null)
        {
            return;
        }

        await EjecutarAsync(async () =>
        {
            var request = new InventarioRequest(inventario.Nombre, inventario.SucursalId, !inventario.Activo);
            await _inventarioApiService.ActualizarAsync(inventario.Id, request);

            var indice = Inventarios.IndexOf(inventario);
            if (indice >= 0)
            {
                Inventarios[indice] = inventario with { Activo = !inventario.Activo };
            }
        });
    }
}
