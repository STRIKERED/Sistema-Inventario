using CommunityToolkit.Mvvm.ComponentModel;
using Inventario.Core.Dtos;

namespace Inventario.Desktop.Models;

/// <summary>Envuelve un InventarioDto con un checkbox de selección, para el multi-select de
/// Inventarios asignados a un Usuario (ver UsuariosViewModel).</summary>
public partial class InventarioSeleccionable : ObservableObject
{
    public int Id { get; }
    public string Nombre { get; }
    public string? SucursalNombre { get; }

    [ObservableProperty]
    private bool seleccionado;

    public InventarioSeleccionable(InventarioDto inventario, bool seleccionado = false)
    {
        Id = inventario.Id;
        Nombre = inventario.Nombre;
        SucursalNombre = inventario.SucursalNombre;
        Seleccionado = seleccionado;
    }
}
