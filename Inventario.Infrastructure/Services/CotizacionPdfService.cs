using Inventario.Core.Entities;
using Inventario.Core.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Inventario.Infrastructure.Services;

/// <summary>Genera el PDF de una cotización usando QuestPDF.</summary>
public class CotizacionPdfService : ICotizacionPdfService
{
    private readonly IConfiguracionImpresionRepository _configuracionRepository;

    static CotizacionPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public CotizacionPdfService(IConfiguracionImpresionRepository configuracionRepository)
    {
        _configuracionRepository = configuracionRepository;
    }

    public async Task<byte[]> GenerarPdfAsync(Cotizacion cotizacion)
    {
        var configuracion = await _configuracionRepository.ObtenerPorInventarioAsync(cotizacion.InventarioId);

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Row(row =>
                {
                    if (!string.IsNullOrWhiteSpace(configuracion?.LogoRutaPdf) && File.Exists(configuracion.LogoRutaPdf))
                    {
                        row.ConstantItem(60).Image(configuracion.LogoRutaPdf);
                    }

                    row.RelativeItem().Column(column =>
                    {
                        column.Item().Text(string.IsNullOrWhiteSpace(configuracion?.EncabezadoTicket)
                                ? "Cotización"
                                : configuracion.EncabezadoTicket)
                            .FontSize(20).Bold();
                        column.Item().Text($"Folio: {cotizacion.Folio}");
                        column.Item().Text($"Fecha: {cotizacion.FechaCreacion:dd/MM/yyyy}");

                        if (!string.IsNullOrWhiteSpace(cotizacion.ClienteNombre))
                        {
                            column.Item().Text($"Cliente: {cotizacion.ClienteNombre}");
                        }

                        if (!string.IsNullOrWhiteSpace(cotizacion.ClienteContacto))
                        {
                            column.Item().Text($"Contacto: {cotizacion.ClienteContacto}");
                        }

                        if (cotizacion.FechaVigencia is not null)
                        {
                            column.Item().Text($"Vigente hasta: {cotizacion.FechaVigencia:dd/MM/yyyy}");
                        }
                    });
                });

                page.Content().PaddingVertical(15).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(4);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Producto").Bold();
                        header.Cell().Text("Cant.").Bold();
                        header.Cell().AlignRight().Text("Precio unitario").Bold();
                        header.Cell().AlignRight().Text("Importe").Bold();
                    });

                    foreach (var detalle in cotizacion.Detalles)
                    {
                        var nombre = detalle.Producto?.Nombre ?? $"Producto #{detalle.ProductoId}";
                        var importe = detalle.Cantidad * detalle.PrecioUnitario;

                        table.Cell().Text(nombre);
                        table.Cell().Text(detalle.Cantidad.ToString());
                        table.Cell().AlignRight().Text(detalle.PrecioUnitario.ToString("C2"));
                        table.Cell().AlignRight().Text(importe.ToString("C2"));
                    }
                });

                page.Footer().Column(column =>
                {
                    column.Item().AlignRight().Column(totales =>
                    {
                        totales.Item().Text($"Subtotal: {cotizacion.Subtotal:C2}");
                        totales.Item().Text($"Descuento: {cotizacion.Descuento:C2}");
                        totales.Item().Text($"Impuestos: {cotizacion.Impuestos:C2}");
                        totales.Item().Text($"Total: {cotizacion.Total:C2}").Bold().FontSize(13);
                    });

                    if (!string.IsNullOrWhiteSpace(configuracion?.PiePaginaTicket))
                    {
                        column.Item().PaddingTop(10).AlignCenter().Text(configuracion.PiePaginaTicket);
                    }
                });
            });
        });

        return documento.GeneratePdf();
    }
}
