using Inventario.Desktop.ViewModels;

namespace Inventario.Desktop.Views;

public partial class RespaldoPage : ContentPage
{
    public RespaldoPage(RespaldoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
