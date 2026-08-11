using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Inventario.Core.Dtos;
using Inventario.Desktop.Services.Api;
using Inventario.Desktop.Services.Sesion;

namespace Inventario.Desktop.ViewModels;

public partial class CajaViewModel : BaseViewModel
{
    private readonly ICajaApiService _cajaApiService;
    private readonly IVentaApiService _ventaApiService;

    public CajaViewModel(ICajaApiService cajaApiService, IVentaApiService ventaApiService, ISessionService sessionService)
        : base(sessionService)
    {
        _cajaApiService = cajaApiService;
        _ventaApiService = ventaApiService;
    }

    public ObservableCollection<CajaDto> Cajas { get; } = new();
    public ObservableCollection<VentaDto> VentasDelTurno { get; } = new();

    [ObservableProperty]
    private CajaDto? cajaSeleccionada;

    [ObservableProperty]
    private CorteDeCajaDto? corteAbierto;

    [ObservableProperty]
    private decimal montoInicial;

    [ObservableProperty]
    private decimal montoFinalContado;

    public bool HayCorteAbierto => CorteAbierto is not null;

    [RelayCommand]
    private async Task CargarAsync()
    {
        await EjecutarAsync(async () =>
        {
            if (SessionService.SucursalOperativaId is null)
            {
                MensajeError = "No hay una sucursal activa en la sesión.";
                return;
            }

            Cajas.Clear();
            foreach (var caja in await _cajaApiService.ObtenerPorSucursalAsync(SessionService.SucursalOperativaId.Value))
            {
                Cajas.Add(caja);
            }

            CajaSeleccionada ??= Cajas.FirstOrDefault();
            await CargarCorteAbiertoAsync();
        });
    }

    partial void OnCajaSeleccionadaChanged(CajaDto? value) => _ = CargarCorteAbiertoAsync();

    private async Task CargarCorteAbiertoAsync()
    {
        VentasDelTurno.Clear();
        MontoFinalContado = 0;

        if (CajaSeleccionada is null)
        {
            CorteAbierto = null;
            return;
        }

        CorteAbierto = await _cajaApiService.ObtenerCorteAbiertoAsync(CajaSeleccionada.Id);
        OnPropertyChanged(nameof(HayCorteAbierto));

        if (CorteAbierto is not null)
        {
            foreach (var venta in await _ventaApiService.ObtenerPorCorteDeCajaAsync(CorteAbierto.Id))
            {
                VentasDelTurno.Add(venta);
            }
        }
    }

    [RelayCommand]
    private async Task AbrirCorteAsync()
    {
        if (CajaSeleccionada is null || SessionService.UsuarioId is null)
        {
            return;
        }

        await EjecutarAsync(async () =>
        {
            var request = new AbrirCorteRequest(CajaSeleccionada.Id, SessionService.UsuarioId.Value, MontoInicial);
            CorteAbierto = await _cajaApiService.AbrirCorteAsync(request);
            MontoInicial = 0;
            OnPropertyChanged(nameof(HayCorteAbierto));
        });
    }

    [RelayCommand]
    private async Task CerrarCorteAsync()
    {
        if (CorteAbierto is null || Shell.Current is null)
        {
            return;
        }

        var confirmar = await Shell.Current.DisplayAlertAsync(
            "Cerrar corte de caja",
            $"Monto contado: {MontoFinalContado:C2}\n¿Confirmas el cierre? Esta acción no se puede deshacer.",
            "Cerrar corte", "Cancelar");

        if (!confirmar)
        {
            return;
        }

        await EjecutarAsync(async () =>
        {
            var cerrado = await _cajaApiService.CerrarCorteAsync(CorteAbierto!.Id, new CerrarCorteRequest(MontoFinalContado));

            await Shell.Current.DisplayAlertAsync(
                "Corte cerrado",
                $"Sistema: {cerrado.MontoFinalSistema:C2}\nContado: {cerrado.MontoFinalContado:C2}\nDiferencia: {cerrado.Diferencia:C2}",
                "Aceptar");

            CorteAbierto = null;
            MontoFinalContado = 0;
            VentasDelTurno.Clear();
            OnPropertyChanged(nameof(HayCorteAbierto));
        });
    }
}
