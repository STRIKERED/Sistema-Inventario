using System.ComponentModel.DataAnnotations;

namespace Inventario.Core.Dtos;

public record ConfiguracionImpresionDto(
    int Id,
    int InventarioId,
    string? InventarioNombre,
    string NombreImpresora,
    int AnchoTicketMm,
    string? EncabezadoTicket,
    string? PiePaginaTicket,
    string? LogoRutaPdf);

public record ConfiguracionImpresionRequest(
    [Range(1, int.MaxValue)] int InventarioId,
    [Required, StringLength(200)] string NombreImpresora,
    int AnchoTicketMm,
    [StringLength(200)] string? EncabezadoTicket,
    [StringLength(200)] string? PiePaginaTicket,
    [StringLength(500)] string? LogoRutaPdf);
