using Inventario.Core.Entities;

namespace Inventario.Core.Interfaces;

/// <summary>Genera documentos PDF de cotizaciones.</summary>
public interface ICotizacionPdfService
{
    /// <summary>
    /// Genera el PDF de la cotización y devuelve su contenido en bytes. Si el Inventario de la
    /// cotización tiene una <see cref="ConfiguracionImpresion"/> asociada, se usa su encabezado,
    /// pie de página y logo; si no, se usan valores genéricos.
    /// </summary>
    Task<byte[]> GenerarPdfAsync(Cotizacion cotizacion);
}
