namespace Inventario.Desktop.Services.Escaneo;

public class BarcodeInputService : IBarcodeInputService
{
    // Un lector de código de barras "wedge" (emulación de teclado) escribe sus caracteres en ráfaga:
    // todo el código llega en unos pocos milisegundos. Se usa un timer de inactividad en vez de medir
    // el tiempo ENTRE caracteres (más frágil y dependiente del modelo de lector): mientras el texto
    // siga cambiando se reinicia; si pasan UmbralInactividadMs sin cambios, se da el escaneo por
    // terminado. LongitudMinima filtra el caso de alguien tecleando una sola tecla al azar.
    private const int UmbralInactividadMs = 120;
    private const int LongitudMinima = 3;

    private readonly System.Timers.Timer _timer;
    private string _textoActual = string.Empty;
    private bool _disposed;

    public event EventHandler<string>? CodigoEscaneado;

    public BarcodeInputService()
    {
        _timer = new System.Timers.Timer(UmbralInactividadMs) { AutoReset = false };
        _timer.Elapsed += (_, _) => Finalizar();
    }

    public void NotificarTexto(string? textoActual)
    {
        _textoActual = textoActual ?? string.Empty;
        _timer.Stop();

        if (_textoActual.Length > 0)
        {
            _timer.Start();
        }
    }

    public void FinalizarConTexto(string? textoActual)
    {
        _textoActual = textoActual ?? string.Empty;
        _timer.Stop();
        Finalizar();
    }

    public void Cancelar()
    {
        _timer.Stop();
        _textoActual = string.Empty;
    }

    private void Finalizar()
    {
        var codigo = _textoActual.Trim();
        _textoActual = string.Empty;

        if (codigo.Length >= LongitudMinima)
        {
            CodigoEscaneado?.Invoke(this, codigo);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _timer.Dispose();
        _disposed = true;
    }
}
