using Inventario.Core.Dtos;
using Inventario.Desktop.ViewModels;

namespace Inventario.Desktop.Views;

public partial class UsuariosPage : ContentPage
{
    private readonly UsuariosViewModel _viewModel;

    public UsuariosPage(UsuariosViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.CargarCommand.Execute(null);
    }

    // El Switch se deja en Mode=OneWay a propósito (UsuarioDto es un record inmutable, no se le puede
    // escribir por binding). El toggle real pasa por el comando, que llama a la API y solo si responde
    // bien reemplaza el item en la lista; si el Switch fuera TwoWay, se vería "activado" aunque la
    // llamada fallara.
    private void OnActivoToggled(object? sender, ToggledEventArgs e)
    {
        if (sender is Switch { BindingContext: UsuarioDto usuario })
        {
            if (_viewModel.CambiarActivoCommand.CanExecute(usuario))
            {
                _viewModel.CambiarActivoCommand.Execute(usuario);
            }
        }
    }
}
