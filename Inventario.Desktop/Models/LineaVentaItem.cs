using CommunityToolkit.Mvvm.ComponentModel;
using Inventario.Core.Dtos;

namespace Inventario.Desktop.Models;

/// <summary>Línea del carrito en la pantalla de Venta. Es un modelo de vista, no un Dto de la API:
/// el precio y el total se recalculan aquí solo para mostrarlos; el servidor los vuelve a calcular
/// de forma autoritativa al confirmar la venta (ver CrearVentaRequest / DetalleVentaRequest).</summary>
public partial class LineaVentaItem : ObservableObject
{
    public int ProductoId { get; }
    public string ProductoNombre { get; }
    public decimal PrecioUnitario { get; }

    [ObservableProperty]
    private int cantidad;

    [ObservableProperty]
    private decimal descuentoUnitario;

    public decimal Importe => Cantidad * (PrecioUnitario - DescuentoUnitario);

    public LineaVentaItem(ProductoDto producto, int cantidadInicial = 1)
    {
        ProductoId = producto.Id;
        ProductoNombre = producto.Nombre;
        PrecioUnitario = producto.PrecioVenta;
        Cantidad = cantidadInicial;
    }

    partial void OnCantidadChanged(int value) => OnPropertyChanged(nameof(Importe));

    partial void OnDescuentoUnitarioChanged(decimal value) => OnPropertyChanged(nameof(Importe));

    public DetalleVentaRequest AConsulta() => new(ProductoId, Cantidad, DescuentoUnitario);
}
