using Inventario.Desktop.ViewModels;

namespace Inventario.Desktop.Views;

public partial class CajaPage : ContentPage
{
    private readonly CajaViewModel _viewModel;

    public CajaPage(CajaViewModel viewModel)
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
