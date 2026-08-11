using System.Windows.Input;
using Inventario.Desktop.Services.Escaneo;

namespace Inventario.Desktop.Controls;

/// <summary>
/// Campo de captura reutilizable para Venta y Cotizaciones. Encapsula un IBarcodeInputService propio
/// (no se resuelve por DI: es una utilidad de UI sin dependencias, atada 1:1 al ciclo de vida de este
/// control) y expone un único comando bindable con el código ya depurado, para que la ViewModel no
/// tenga que saber nada de temporizadores ni de TextChanged/Completed.
/// </summary>
public partial class EntradaCodigoBarras : ContentView
{
    public static readonly BindableProperty ComandoEscaneoProperty = BindableProperty.Create(
        nameof(ComandoEscaneo), typeof(ICommand), typeof(EntradaCodigoBarras));

    public ICommand? ComandoEscaneo
    {
        get => (ICommand?)GetValue(ComandoEscaneoProperty);
        set => SetValue(ComandoEscaneoProperty, value);
    }

    private readonly IBarcodeInputService _barcodeInputService = new BarcodeInputService();

    public EntradaCodigoBarras()
    {
        InitializeComponent();
        _barcodeInputService.CodigoEscaneado += OnCodigoEscaneado;
        Loaded += (_, _) => Enfocar();
    }

    /// <summary>Devuelve el foco al campo; llamar tras cerrar un diálogo/alerta para que el
    /// siguiente disparo del lector no se pierda por falta de foco.</summary>
    public void Enfocar() => EntryCodigo.Focus();

    private void OnTextChanged(object? sender, TextChangedEventArgs e) =>
        _barcodeInputService.NotificarTexto(e.NewTextValue);

    private void OnCompleted(object? sender, EventArgs e) =>
        _barcodeInputService.FinalizarConTexto(EntryCodigo.Text);

    private void OnCodigoEscaneado(object? sender, string codigo)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            EntryCodigo.Text = string.Empty;
            Enfocar();

            if (ComandoEscaneo?.CanExecute(codigo) == true)
            {
                ComandoEscaneo.Execute(codigo);
            }
        });
    }
}
