using Inventario.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Inventario.Infrastructure.Services;

public class CalculadoraTotalesService : ICalculadoraTotalesService
{
    private readonly decimal _tasaIva;

    public CalculadoraTotalesService(IConfiguration configuration)
    {
        // Parseo manual (en vez de GetValue<T>) para no depender del paquete Configuration.Binder,
        // que este proyecto no referencia; mismo patrón que JwtTokenService con Jwt:ExpirationMinutes.
        var valor = configuration["Negocio:TasaIva"];
        _tasaIva = decimal.TryParse(valor, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var tasa) ? tasa : 0.16m;
    }

    public (decimal Impuestos, decimal Total) Calcular(decimal subtotal, decimal descuento)
    {
        var baseGravable = Math.Max(subtotal - descuento, 0m);
        var impuestos = Math.Round(baseGravable * _tasaIva, 2, MidpointRounding.AwayFromZero);
        var total = baseGravable + impuestos;
        return (impuestos, total);
    }
}
