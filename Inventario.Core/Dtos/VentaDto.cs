using System.ComponentModel.DataAnnotations;
using Inventario.Core.Enums;

namespace Inventario.Core.Dtos;

public record DetalleVentaDto(
    int Id,
    int ProductoId,
    string? ProductoNombre,
    int Cantidad,
    decimal PrecioUnitario,
    decimal DescuentoUnitario,
    decimal ImporteLinea);

public record VentaDto(
    int Id,
    string Folio,
    DateTime Fecha,
    MetodoPago MetodoPago,
    decimal Subtotal,
    decimal Descuento,
    decimal Impuestos,
    decimal Total,
    int InventarioId,
    string? InventarioNombre,
    int CorteDeCajaId,
    int UsuarioId,
    string? UsuarioNombre,
    bool Cancelada,
    IReadOnlyList<DetalleVentaDto> Detalles);

// Sin Folio (lo genera el servidor tras el insert) ni PrecioUnitario (se toma de Producto.PrecioVenta
// en el momento de la venta, para que el cliente no pueda manipular el precio).
public record DetalleVentaRequest(
    [Range(1, int.MaxValue)] int ProductoId,
    [Range(1, int.MaxValue)] int Cantidad,
    [Range(0, double.MaxValue)] decimal DescuentoUnitario);

public record CrearVentaRequest(
    MetodoPago MetodoPago,
    [Range(1, int.MaxValue)] int InventarioId,
    [Range(1, int.MaxValue)] int CorteDeCajaId,
    [Range(1, int.MaxValue)] int UsuarioId,
    [Required, MinLength(1)] List<DetalleVentaRequest> Detalles);
