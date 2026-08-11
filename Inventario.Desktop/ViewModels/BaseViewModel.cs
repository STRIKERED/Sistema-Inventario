using CommunityToolkit.Mvvm.ComponentModel;
using Inventario.Desktop.Services.Http;
using Inventario.Desktop.Services.Sesion;

namespace Inventario.Desktop.ViewModels;

/// <summary>
/// Centraliza lo que toda pantalla necesita: estado de "ocupado", mensaje de error y manejo uniforme
/// de ApiException. En particular, un 401 (token vencido/ inválido) siempre cierra sesión y regresa
/// a Login, sin que cada ViewModel tenga que acordarse de revisarlo.
/// </summary>
public abstract partial class BaseViewModel : ObservableObject
{
    protected readonly ISessionService SessionService;

    protected BaseViewModel(ISessionService sessionService)
    {
        SessionService = sessionService;
    }

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string? mensajeError;

    protected async Task EjecutarAsync(Func<Task> accion)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        MensajeError = null;
        try
        {
            await accion();
        }
        catch (ApiException ex) when (ex.StatusCode == 401)
        {
            await SessionService.CerrarSesionAsync();
            if (Shell.Current is not null)
            {
                await Shell.Current.GoToAsync("//login");
            }
        }
        catch (ApiException ex)
        {
            MensajeError = ex.Message;
        }
        catch (Exception ex)
        {
            MensajeError = "Ocurrió un error inesperado: " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
