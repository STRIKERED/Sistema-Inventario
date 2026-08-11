using System.ComponentModel.DataAnnotations;
using Inventario.Core.Enums;

namespace Inventario.Core.Dtos;

public record DetalleCotizacionDto(
    int Id,
    int ProductoId,
    string? ProductoNombre,
    int Cantidad,
    decimal PrecioUnitario,
    decimal ImporteLinea);

public record CotizacionDto(
    int Id,
    string Folio,
    string? ClienteNombre,
    string? ClienteContacto,
    DateTime FechaCreacion,
    DateTime? FechaVigencia,
    EstadoCotizacion Estado,
    decimal Subtotal,
    decimal Descuento,
    decimal Impuestos,
    decimal Total,
    int SucursalId,
    string? SucursalNombre,
    int UsuarioId,
    string? UsuarioNombre,
    IReadOnlyList<DetalleCotizacionDto> Detalles);

// Sin PrecioUnitario: se toma de Producto.PrecioVenta al momento de crear la cotización.
public record DetalleCotizacionRequest(
    [property: Range(1, int.MaxValue)] int ProductoId,
    [property: Range(1, int.MaxValue)] int Cantidad);

public record CrearCotizacionRequest(
    string? ClienteNombre,
    string? ClienteContacto,
    [property: Range(1, int.MaxValue)] int SucursalId,
    [property: Range(1, int.MaxValue)] int UsuarioId,
    DateTime? FechaVigencia,
    [property: Range(0, double.MaxValue)] decimal Descuento,
    [property: Required, MinLength(1)] List<DetalleCotizacionRequest> Detalles);

public record ConvertirAVentaRequest(
    [property: Range(1, int.MaxValue)] int UsuarioId,
    [property: Range(1, int.MaxValue)] int CorteDeCajaId,
    MetodoPago MetodoPago);
