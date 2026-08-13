using Inventario.Core.Dtos;
using Inventario.Core.Entities;

namespace Inventario.Core.Mapping;

public static class ConfiguracionImpresionMapping
{
    public static ConfiguracionImpresionDto ToDto(this ConfiguracionImpresion configuracion) =>
        new(
            configuracion.Id,
            configuracion.InventarioId,
            configuracion.Inventario?.Nombre,
            configuracion.NombreImpresora,
            configuracion.AnchoTicketMm,
            configuracion.EncabezadoTicket,
            configuracion.PiePaginaTicket,
            configuracion.LogoRutaPdf);

    public static ConfiguracionImpresion ToEntity(this ConfiguracionImpresionRequest request) =>
        new()
        {
            InventarioId = request.InventarioId,
            NombreImpresora = request.NombreImpresora,
            AnchoTicketMm = request.AnchoTicketMm,
            EncabezadoTicket = request.EncabezadoTicket,
            PiePaginaTicket = request.PiePaginaTicket,
            LogoRutaPdf = request.LogoRutaPdf
        };

    public static void AplicarA(this ConfiguracionImpresionRequest request, ConfiguracionImpresion configuracion)
    {
        configuracion.InventarioId = request.InventarioId;
        configuracion.NombreImpresora = request.NombreImpresora;
        configuracion.AnchoTicketMm = request.AnchoTicketMm;
        configuracion.EncabezadoTicket = request.EncabezadoTicket;
        configuracion.PiePaginaTicket = request.PiePaginaTicket;
        configuracion.LogoRutaPdf = request.LogoRutaPdf;
    }
}
