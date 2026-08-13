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
    int InventarioId,
    string? InventarioNombre,
    int UsuarioId,
    string? UsuarioNombre,
    IReadOnlyList<DetalleCotizacionDto> Detalles);

// Sin PrecioUnitario: se toma de Producto.PrecioVenta al momento de crear la cotización.
public record DetalleCotizacionRequest(
    [Range(1, int.MaxValue)] int ProductoId,
    [Range(1, int.MaxValue)] int Cantidad);

public record CrearCotizacionRequest(
    string? ClienteNombre,
    string? ClienteContacto,
    [Range(1, int.MaxValue)] int InventarioId,
    [Range(1, int.MaxValue)] int UsuarioId,
    DateTime? FechaVigencia,
    [Range(0, double.MaxValue)] decimal Descuento,
    [Required, MinLength(1)] List<DetalleCotizacionRequest> Detalles);

public record ConvertirAVentaRequest(
    [Range(1, int.MaxValue)] int UsuarioId,
    [Range(1, int.MaxValue)] int CorteDeCajaId,
    MetodoPago MetodoPago);

// Sin Detalles/InventarioId: una vez creada, las líneas y el importe de una cotización no se tocan
// (para eso se cancela y se hace una nueva) — esto solo edita los datos de "encabezado".
public record ActualizarCotizacionRequest(
    string? ClienteNombre,
    string? ClienteContacto,
    DateTime? FechaVigencia,
    EstadoCotizacion Estado);
