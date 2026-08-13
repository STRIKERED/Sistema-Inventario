using Inventario.Desktop.ViewModels;

namespace Inventario.Desktop.Views;

public partial class ConfiguracionImpresionPage : ContentPage
{
    private readonly ConfiguracionImpresionViewModel _viewModel;

    public ConfiguracionImpresionPage(ConfiguracionImpresionViewModel viewModel)
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
