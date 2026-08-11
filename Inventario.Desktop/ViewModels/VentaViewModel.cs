using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Inventario.Core.Dtos;
using Inventario.Core.Enums;
using Inventario.Desktop.Models;
using Inventario.Desktop.Services.Api;
using Inventario.Desktop.Services.Sesion;

namespace Inventario.Desktop.ViewModels;

public partial class VentaViewModel : BaseViewModel
{
    // Tasa usada SOLO para la vista previa en pantalla mientras se arma el carrito. El total real,
    // autoritativo, lo calcula el servidor con la tasa configurada en Inventario.Api (appsettings
    // Negocio:TasaIva) al confirmar la venta — puede diferir si cambia la config del lado del servidor.
    private const decimal TasaIvaVistaPrevia = 0.16m;

    private readonly IProductoApiService _productoApiService;
    private readonly IVentaApiService _ventaApiService;
    private readonly ICajaApiService _cajaApiService;

    public VentaViewModel(
        IProductoApiService productoApiService,
        IVentaApiService ventaApiService,
        ICajaApiService cajaApiService,
        ISessionService sessionService)
        : base(sessionService)
    {
        _productoApiService = productoApiService;
        _ventaApiService = ventaApiService;
        _cajaApiService = cajaApiService;

        Carrito.CollectionChanged += (_, _) => RecalcularTotales();
    }

    public ObservableCollection<CajaDto> Cajas { get; } = new();
    public ObservableCollection<LineaVentaItem> Carrito { get; } = new();
    public IReadOnlyList<MetodoPago> MetodosPago { get; } = Enum.GetValues<MetodoPago>();

    [ObservableProperty]
    private CajaDto? cajaSeleccionada;

    [ObservableProperty]
    private CorteDeCajaDto? corteAbierto;

    [ObservableProperty]
    private MetodoPago metodoPago = MetodoPago.Efectivo;

    [ObservableProperty]
    private decimal subtotalEstimado;

    [ObservableProperty]
    private decimal descuentoEstimado;

    [ObservableProperty]
    private decimal impuestosEstimados;

    [ObservableProperty]
    private decimal totalEstimado;

    // No depende de IsBusy a propósito: EjecutarAsync ya bloquea la reentrada de comandos mientras hay
    // uno en curso, y así se evita tener que renotificar este cálculo cada vez que IsBusy cambia
    // (IsBusy se declara en BaseViewModel, así que un partial OnIsBusyChanged no puede "engancharse"
    // desde esta subclase). El estado ocupado se refleja aparte en la UI con un ActivityIndicator.
    public bool PuedeCobrar => CorteAbierto is not null && Carrito.Count > 0;

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
        if (CajaSeleccionada is null)
        {
            CorteAbierto = null;
            return;
        }

        CorteAbierto = await _cajaApiService.ObtenerCorteAbiertoAsync(CajaSeleccionada.Id);
        OnPropertyChanged(nameof(PuedeCobrar));
    }

    [RelayCommand]
    private async Task AgregarPorCodigoAsync(string codigo)
    {
        await EjecutarAsync(async () =>
        {
            var producto = await _productoApiService.ObtenerPorCodigoBarrasAsync(codigo);
            if (producto is null)
            {
                MensajeError = $"No se encontró ningún producto con el código '{codigo}'.";
                return;
            }

            if (!producto.Activo)
            {
                MensajeError = $"El producto '{producto.Nombre}' está inactivo.";
                return;
            }

            var lineaExistente = Carrito.FirstOrDefault(l => l.ProductoId == producto.Id);
            if (lineaExistente is not null)
            {
                lineaExistente.Cantidad++;
            }
            else
            {
                var linea = new LineaVentaItem(producto);
                linea.PropertyChanged += (_, _) => RecalcularTotales();
                Carrito.Add(linea);
            }

            RecalcularTotales();
        });
    }

    [RelayCommand]
    private void QuitarLinea(LineaVentaItem? linea)
    {
        if (linea is not null)
        {
            Carrito.Remove(linea);
        }
    }

    [RelayCommand(CanExecute = nameof(PuedeCobrar))]
    private async Task CobrarAsync()
    {
        if (CajaSeleccionada is null || CorteAbierto is null || SessionService.UsuarioId is null || SessionService.SucursalOperativaId is null)
        {
            return;
        }

        VentaDto? ventaCreada = null;

        await EjecutarAsync(async () =>
        {
            var request = new CrearVentaRequest(
                MetodoPago,
                SessionService.SucursalOperativaId.Value,
                CorteAbierto.Id,
                SessionService.UsuarioId.Value,
                Carrito.Select(l => l.AConsulta()).ToList());

            ventaCreada = await _ventaApiService.CrearAsync(request);
            Carrito.Clear();
        });

        if (ventaCreada is null || Shell.Current is null)
        {
            return;
        }

        var quiereImprimir = await Shell.Current.DisplayAlertAsync(
            "Venta registrada",
            $"Folio {ventaCreada.Folio} — Total: {ventaCreada.Total:C2}\n¿Imprimir ticket?",
            "Imprimir", "Cerrar");

        if (!quiereImprimir)
        {
            return;
        }

        var impresora = await Shell.Current.DisplayPromptAsync("Imprimir ticket", "Nombre de la impresora térmica:");
        if (string.IsNullOrWhiteSpace(impresora))
        {
            return;
        }

        await EjecutarAsync(() => _ventaApiService.ImprimirAsync(ventaCreada.Id, impresora));
    }

    private void RecalcularTotales()
    {
        SubtotalEstimado = Carrito.Sum(l => l.Cantidad * l.PrecioUnitario);
        DescuentoEstimado = Carrito.Sum(l => l.Cantidad * l.DescuentoUnitario);

        var baseGravable = Math.Max(SubtotalEstimado - DescuentoEstimado, 0m);
        ImpuestosEstimados = Math.Round(baseGravable * TasaIvaVistaPrevia, 2, MidpointRounding.AwayFromZero);
        TotalEstimado = baseGravable + ImpuestosEstimados;

        OnPropertyChanged(nameof(PuedeCobrar));
        CobrarCommand.NotifyCanExecuteChanged();
    }
}
