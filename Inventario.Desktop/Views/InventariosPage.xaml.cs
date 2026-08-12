using Inventario.Core.Dtos;
using Inventario.Desktop.ViewModels;

namespace Inventario.Desktop.Views;

public partial class InventariosPage : ContentPage
{
    private readonly InventariosViewModel _viewModel;

    public InventariosPage(InventariosViewModel viewModel)
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

    // Ver el comentario equivalente en UsuariosPage.xaml.cs: IsToggled se deja en Mode=OneWay porque
    // InventarioDto es un record inmutable; el toggle real pasa por el comando.
    private void OnActivoToggled(object? sender, ToggledEventArgs e)
    {
        if (sender is Switch { BindingContext: InventarioDto inventario })
        {
            if (_viewModel.CambiarActivoCommand.CanExecute(inventario))
            {
                _viewModel.CambiarActivoCommand.Execute(inventario);
            }
        }
    }
}
