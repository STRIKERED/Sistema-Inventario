using Inventario.Desktop.ViewModels;

namespace Inventario.Desktop.Views;

public partial class VentaPage : ContentPage
{
    private readonly VentaViewModel _viewModel;

    public VentaPage(VentaViewModel viewModel)
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
}
