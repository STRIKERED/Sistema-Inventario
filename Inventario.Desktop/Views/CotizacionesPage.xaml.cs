using Inventario.Desktop.ViewModels;

namespace Inventario.Desktop.Views;

public partial class CotizacionesPage : ContentPage
{
    private readonly CotizacionesViewModel _viewModel;

    public CotizacionesPage(CotizacionesViewModel viewModel)
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
