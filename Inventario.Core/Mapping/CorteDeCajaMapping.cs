using Inventario.Core.Dtos;
using Inventario.Core.Entities;

namespace Inventario.Core.Mapping;

public static class CorteDeCajaMapping
{
    public static CorteDeCajaDto ToDto(this CorteDeCaja corte) =>
        new(corte.Id, corte.MontoInicial, corte.MontoFinalContado, corte.MontoFinalSistema, corte.Diferencia,
            corte.Estado, corte.FechaApertura, corte.FechaCierre, corte.CajaId, corte.Caja?.Nombre,
            corte.UsuarioId, corte.Usuario?.NombreUsuario);

    public static IEnumerable<CorteDeCajaDto> ToDto(this IEnumerable<CorteDeCaja> cortes) =>
        cortes.Select(c => c.ToDto());
}
