using Inventario.Core.Entities;
using Inventario.Core.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Inventario.Infrastructure.Services;

/// <summary>Genera el PDF de una cotización usando QuestPDF.</summary>
public class CotizacionPdfService : ICotizacionPdfService
{
    static CotizacionPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerarPdf(Cotizacion cotizacion)
    {
        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(column =>
                {
                    column.Item().Text("Cotización").FontSize(20).Bold();
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

                page.Footer().AlignRight().Column(column =>
                {
                    column.Item().Text($"Subtotal: {cotizacion.Subtotal:C2}");
                    column.Item().Text($"Descuento: {cotizacion.Descuento:C2}");
                    column.Item().Text($"Impuestos: {cotizacion.Impuestos:C2}");
                    column.Item().Text($"Total: {cotizacion.Total:C2}").Bold().FontSize(13);
                });
            });
        });

        return documento.GeneratePdf();
    }
}
