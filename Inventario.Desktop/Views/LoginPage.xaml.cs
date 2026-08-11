using Inventario.Desktop.ViewModels;

namespace Inventario.Desktop.Views;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel _viewModel;

    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        VersionLabel.Text = $"Versión {AppInfo.Current.VersionString}";
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.CargarEstadoCommand.Execute(null);
    }

    private void OnPasswordCompleted(object? sender, EventArgs e)
    {
        if (_viewModel.IniciarSesionCommand.CanExecute(null))
        {
            _viewModel.IniciarSesionCommand.Execute(null);
        }
    }

    private void OnConfirmarPasswordCompleted(object? sender, EventArgs e)
    {
        if (_viewModel.CrearAdministradorInicialCommand.CanExecute(null))
        {
            _viewModel.CrearAdministradorInicialCommand.Execute(null);
        }
    }
}
