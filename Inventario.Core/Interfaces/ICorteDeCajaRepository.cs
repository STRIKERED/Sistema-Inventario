using Inventario.Core.Entities;

namespace Inventario.Core.Interfaces;

public interface ICorteDeCajaRepository
{
    Task<CorteDeCaja?> ObtenerPorIdAsync(int id);
    Task<CorteDeCaja?> ObtenerAbiertoPorCajaAsync(int cajaId);
    Task<IEnumerable<CorteDeCaja>> ObtenerPorCajaAsync(int cajaId);
    Task<CorteDeCaja> CrearAsync(CorteDeCaja corteDeCaja);
    Task ActualizarAsync(CorteDeCaja corteDeCaja);
}
