using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Inventario.Core.Dtos;
using Inventario.Desktop.Services.Api;
using Inventario.Desktop.Services.Sesion;

namespace Inventario.Desktop.ViewModels;

/// <summary>
/// Alta/edición de la <see cref="ConfiguracionImpresionDto"/> del Inventario con el que opera la
/// sesión actual: impresora térmica para tickets (ESC/POS) y encabezado/pie/logo usados también en
/// el PDF de cotizaciones. Solo visible para Administrador/Gerente (ver AppShell.xaml.cs), porque
/// la Api restringe el POST/PUT a esos roles.
/// </summary>
public partial class ConfiguracionImpresionViewModel : BaseViewModel
{
    private readonly IConfiguracionImpresionApiService _configuracionApiService;

    public ConfiguracionImpresionViewModel(IConfiguracionImpresionApiService configuracionApiService, ISessionService sessionService)
        : base(sessionService)
    {
        _configuracionApiService = configuracionApiService;
    }

    public List<int> AnchosDisponiblesMm { get; } = [58, 80];

    private int? _configuracionId;

    [ObservableProperty]
    private string nombreImpresora = string.Empty;

    [ObservableProperty]
    private int anchoTicketMm = 80;

    [ObservableProperty]
    private string? encabezadoTicket;

    [ObservableProperty]
    private string? piePaginaTicket;

    [ObservableProperty]
    private string? logoRutaPdf;

    /// <summary>True si ya existe una configuración guardada para este Inventario (se está editando,
    /// no creando). Solo informativo para la UI.</summary>
    public bool HayConfiguracionExistente => _configuracionId is not null;

    [RelayCommand]
    private async Task CargarAsync()
    {
        await EjecutarAsync(async () =>
        {
            if (SessionService.InventarioOperativoId is null)
            {
                MensajeError = "No hay un inventario activo en la sesión.";
                return;
            }

            var configuracion = await _configuracionApiService.ObtenerPorInventarioAsync(SessionService.InventarioOperativoId.Value);

            _configuracionId = configuracion?.Id;
            NombreImpresora = configuracion?.NombreImpresora ?? string.Empty;
            AnchoTicketMm = configuracion?.AnchoTicketMm ?? 80;
            EncabezadoTicket = configuracion?.EncabezadoTicket;
            PiePaginaTicket = configuracion?.PiePaginaTicket;
            LogoRutaPdf = configuracion?.LogoRutaPdf;
            OnPropertyChanged(nameof(HayConfiguracionExistente));
        });
    }

    [RelayCommand]
    private async Task ElegirLogoAsync()
    {
        var archivo = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Selecciona el logo para las cotizaciones en PDF",
            FileTypes = FilePickerFileType.Images
        });

        if (archivo is not null)
        {
            LogoRutaPdf = archivo.FullPath;
        }
    }

    [RelayCommand]
    private async Task GuardarAsync()
    {
        if (SessionService.InventarioOperativoId is null)
        {
            MensajeError = "No hay un inventario activo en la sesión.";
            return;
        }

        if (string.IsNullOrWhiteSpace(NombreImpresora))
        {
            MensajeError = "El nombre de la impresora es obligatorio (el mismo con el que aparece instalada en Windows).";
            return;
        }

        await EjecutarAsync(async () =>
        {
            var request = new ConfiguracionImpresionRequest(
                SessionService.InventarioOperativoId!.Value,
                NombreImpresora.Trim(),
                AnchoTicketMm,
                string.IsNullOrWhiteSpace(EncabezadoTicket) ? null : EncabezadoTicket.Trim(),
                string.IsNullOrWhiteSpace(PiePaginaTicket) ? null : PiePaginaTicket.Trim(),
                string.IsNullOrWhiteSpace(LogoRutaPdf) ? null : LogoRutaPdf.Trim());

            if (_configuracionId is null)
            {
                var creada = await _configuracionApiService.CrearAsync(request);
                _configuracionId = creada.Id;
                OnPropertyChanged(nameof(HayConfiguracionExistente));
            }
            else
            {
                await _configuracionApiService.ActualizarAsync(_configuracionId.Value, request);
            }

            if (Shell.Current is not null)
            {
                await Shell.Current.DisplayAlertAsync(
                    "Configuración guardada",
                    "Los cambios se usarán en el próximo ticket o cotización que se imprima.",
                    "Aceptar");
            }
        });
    }
}
