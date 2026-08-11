using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Inventario.Core.Dtos;
using Inventario.Core.Enums;
using Inventario.Desktop.Models;
using Inventario.Desktop.Services.Api;
using Inventario.Desktop.Services.Sesion;

namespace Inventario.Desktop.ViewModels;

public partial class CotizacionesViewModel : BaseViewModel
{
    private readonly ICotizacionApiService _cotizacionApiService;
    private readonly IProductoApiService _productoApiService;
    private readonly ICajaApiService _cajaApiService;

    public CotizacionesViewModel(
        ICotizacionApiService cotizacionApiService,
        IProductoApiService productoApiService,
        ICajaApiService cajaApiService,
        ISessionService sessionService)
        : base(sessionService)
    {
        _cotizacionApiService = cotizacionApiService;
        _productoApiService = productoApiService;
        _cajaApiService = cajaApiService;
    }

    public ObservableCollection<CotizacionDto> Cotizaciones { get; } = new();
    public ObservableCollection<LineaCotizacionItem> NuevaCotizacionCarrito { get; } = new();
    public IReadOnlyList<MetodoPago> MetodosPago { get; } = Enum.GetValues<MetodoPago>();

    [ObservableProperty]
    private bool mostrarFormularioNuevaCotizacion;

    [ObservableProperty]
    private string? clienteNombre;

    [ObservableProperty]
    private string? clienteContacto;

    [ObservableProperty]
    private DateTime fechaVigencia = DateTime.Today.AddDays(15);

    [ObservableProperty]
    private decimal descuentoGlobal;

    // Requeridos para poder convertir una cotización en venta (necesita un corte de caja abierto).
    [ObservableProperty]
    private CorteDeCajaDto? corteAbierto;

    [ObservableProperty]
    private MetodoPago metodoPagoConversion = MetodoPago.Efectivo;

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

            Cotizaciones.Clear();
            foreach (var cotizacion in await _cotizacionApiService.ObtenerVigentesAsync(SessionService.SucursalOperativaId.Value))
            {
                Cotizaciones.Add(cotizacion);
            }

            // Se usa la primera caja de la sucursal para saber si hay un corte abierto con el que
            // convertir cotizaciones en venta; si la sucursal maneja varias cajas, igual sirve como
            // señal de "hay operación abierta" y el cajero puede abrir la suya desde la pantalla de Caja.
            var cajas = await _cajaApiService.ObtenerPorSucursalAsync(SessionService.SucursalOperativaId.Value);
            var primeraCaja = cajas.FirstOrDefault();
            CorteAbierto = primeraCaja is null ? null : await _cajaApiService.ObtenerCorteAbiertoAsync(primeraCaja.Id);
        });
    }

    [RelayCommand]
    private void MostrarFormulario() => MostrarFormularioNuevaCotizacion = true;

    [RelayCommand]
    private void CancelarFormulario()
    {
        MostrarFormularioNuevaCotizacion = false;
        ClienteNombre = null;
        ClienteContacto = null;
        DescuentoGlobal = 0;
        FechaVigencia = DateTime.Today.AddDays(15);
        NuevaCotizacionCarrito.Clear();
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

            var existente = NuevaCotizacionCarrito.FirstOrDefault(l => l.ProductoId == producto.Id);
            if (existente is not null)
            {
                existente.Cantidad++;
            }
            else
            {
                NuevaCotizacionCarrito.Add(new LineaCotizacionItem(producto));
            }
        });
    }

    [RelayCommand]
    private void QuitarLinea(LineaCotizacionItem? linea)
    {
        if (linea is not null)
        {
            NuevaCotizacionCarrito.Remove(linea);
        }
    }

    [RelayCommand]
    private async Task GuardarCotizacionAsync()
    {
        if (SessionService.SucursalOperativaId is null || SessionService.UsuarioId is null)
        {
            return;
        }

        if (NuevaCotizacionCarrito.Count == 0)
        {
            MensajeError = "Agrega al menos un producto a la cotización.";
            return;
        }

        await EjecutarAsync(async () =>
        {
            var request = new CrearCotizacionRequest(
                ClienteNombre,
                ClienteContacto,
                SessionService.SucursalOperativaId.Value,
                SessionService.UsuarioId.Value,
                FechaVigencia,
                DescuentoGlobal,
                NuevaCotizacionCarrito.Select(l => l.AConsulta()).ToList());

            var creada = await _cotizacionApiService.CrearAsync(request);
            Cotizaciones.Insert(0, creada);
            CancelarFormularioCommand.Execute(null);
        });
    }

    [RelayCommand]
    private async Task ConvertirAVentaAsync(CotizacionDto? cotizacion)
    {
        if (cotizacion is null || Shell.Current is null)
        {
            return;
        }

        if (CorteAbierto is null || SessionService.UsuarioId is null)
        {
            MensajeError = "No hay un corte de caja abierto: ábrelo desde la pantalla de Caja antes de convertir la cotización.";
            return;
        }

        var confirmar = await Shell.Current.DisplayAlertAsync(
            "Convertir a venta",
            $"¿Convertir la cotización {cotizacion.Folio} (Total {cotizacion.Total:C2}) en una venta?",
            "Convertir", "Cancelar");

        if (!confirmar)
        {
            return;
        }

        await EjecutarAsync(async () =>
        {
            var request = new ConvertirAVentaRequest(SessionService.UsuarioId.Value, CorteAbierto.Id, MetodoPagoConversion);
            var venta = await _cotizacionApiService.ConvertirAVentaAsync(cotizacion.Id, request);

            Cotizaciones.Remove(cotizacion);
            await Shell.Current.DisplayAlertAsync("Venta generada", $"Folio {venta.Folio} — Total {venta.Total:C2}", "Aceptar");
        });
    }
}
