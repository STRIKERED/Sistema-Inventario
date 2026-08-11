using Inventario.Core.Entities;

namespace Inventario.Core.Interfaces;

/// <summary>Genera documentos PDF de cotizaciones.</summary>
public interface ICotizacionPdfService
{
    /// <summary>Genera el PDF de la cotización y devuelve su contenido en bytes.</summary>
    byte[] GenerarPdf(Cotizacion cotizacion);
}
