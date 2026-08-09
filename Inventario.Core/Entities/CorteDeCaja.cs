using Inventario.Core.Enums;

namespace Inventario.Core.Entities;

public class CorteDeCaja
{
    public int Id { get; set; }
    public decimal MontoInicial { get; set; }
    public decimal MontoFinalContado { get; set; }
    public decimal MontoFinalSistema { get; set; }
    public decimal Diferencia { get; set; }
    public EstadoCorteDeCaja Estado { get; set; } = EstadoCorteDeCaja.Abierto;
    public DateTime FechaApertura { get; set; } = DateTime.UtcNow;
    public DateTime? FechaCierre { get; set; }

    public int CajaId { get; set; }
    public Caja Caja { get; set; } = null!;

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}
