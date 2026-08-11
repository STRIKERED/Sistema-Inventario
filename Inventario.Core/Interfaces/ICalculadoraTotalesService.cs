namespace Inventario.Core.Interfaces;

/// <summary>
/// Calcula impuestos y total a partir de un subtotal y un descuento, aplicando la tasa de IVA
/// configurada (appsettings: Negocio:TasaIva). La tasa es fija y global: no varía por producto.
/// </summary>
public interface ICalculadoraTotalesService
{
    (decimal Impuestos, decimal Total) Calcular(decimal subtotal, decimal descuento);
}
