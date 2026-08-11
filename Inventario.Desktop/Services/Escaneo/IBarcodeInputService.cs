namespace Inventario.Desktop.Services.Escaneo;

/// <summary>
/// Detecta cuándo el texto que va llegando a un campo de captura corresponde a un código de barras
/// completo, ya sea porque el lector mandó su terminador (Enter/Tab) o porque dejó de "teclear"
/// (los lectores emulan teclado y escriben en ráfaga, mucho más rápido que una persona).
/// No depende de UI: se alimenta desde el TextChanged/Completed de cualquier control de entrada.
/// </summary>
public interface IBarcodeInputService : IDisposable
{
    /// <summary>
    /// Se dispara con el código ya depurado cuando se detecta un escaneo completo. OJO: puede
    /// dispararse desde un hilo de threadpool (el del temporizador de inactividad), no el de UI;
    /// quien lo consuma debe despachar a MainThread antes de tocar la interfaz.
    /// </summary>
    event EventHandler<string>? CodigoEscaneado;

    /// <summary>Llamar en cada TextChanged del control de captura, con el texto actual completo.</summary>
    void NotificarTexto(string? textoActual);

    /// <summary>Llamar en el Completed del control (Enter/terminador). Finaliza sin esperar al timer.</summary>
    void FinalizarConTexto(string? textoActual);

    /// <summary>Descarta cualquier captura en curso sin disparar el evento.</summary>
    void Cancelar();
}
