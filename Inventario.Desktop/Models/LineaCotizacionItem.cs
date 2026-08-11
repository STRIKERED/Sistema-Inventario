using CommunityToolkit.Mvvm.ComponentModel;
using Inventario.Core.Dtos;

namespace Inventario.Desktop.Models;

/// <summary>Línea del formulario de nueva cotización. Sin descuento por línea: en Cotización el
/// descuento es global (ver CrearCotizacionRequest.Descuento), igual que en la entidad Cotizacion.</summary>
public partial class LineaCotizacionItem : ObservableObject
{
    public int ProductoId { get; }
    public string ProductoNombre { get; }
    public decimal PrecioUnitario { get; }

    [ObservableProperty]
    private int cantidad;

    public decimal Importe => Cantidad * PrecioUnitario;

    public LineaCotizacionItem(ProductoDto producto, int cantidadInicial = 1)
    {
        ProductoId = producto.Id;
        ProductoNombre = producto.Nombre;
        PrecioUnitario = producto.PrecioVenta;
        Cantidad = cantidadInicial;
    }

    partial void OnCantidadChanged(int value) => OnPropertyChanged(nameof(Importe));

    public DetalleCotizacionRequest AConsulta() => new(ProductoId, Cantidad);
}
