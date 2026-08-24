namespace Inventario.Api;

/// <summary>
/// Convierte fechas "de calendario" (sin hora, como llegan de un &lt;input type="date"&gt;) al
/// instante UTC equivalente, para comparar contra columnas guardadas con DateTime.UtcNow
/// (Venta.Fecha, MovimientoInventario.Fecha, ...).
///
/// Sin esto, comparar un límite de calendario tal cual contra una columna en UTC da resultados
/// incorrectos en cualquier zona horaria distinta de UTC: p. ej. en UTC-6, a las 9pm hora local ya
/// son las 3am del día siguiente en UTC, así que una venta de "hoy" (local) puede tener
/// Fecha == mañana en UTC y quedar fuera de un rango "hasta hoy" calculado sin convertir.
/// </summary>
internal static class RangoFechaLocalHelper
{
    /// <summary>Instante UTC del inicio (00:00:00) del día de calendario indicado, en la zona
    /// horaria local del servidor. Null se preserva (sin límite inferior).</summary>
    public static DateTime? InicioDiaAUtc(DateTime? fecha) =>
        fecha is null ? null : ConvertirAUtc(fecha.Value.Date);

    /// <summary>Instante UTC del final (23:59:59.9999999) del día de calendario indicado, en la
    /// zona horaria local del servidor. Null se preserva (sin límite superior).</summary>
    public static DateTime? FinDiaAUtc(DateTime? fecha) =>
        fecha is null ? null : ConvertirAUtc(fecha.Value.Date.AddDays(1).AddTicks(-1));

    /// <summary>Rango [inicio de "desde", fin de "hasta"], con ambos límites por default en el día
    /// de hoy (local) si se omiten — para endpoints donde "sin filtro" significa "hoy".</summary>
    public static (DateTime DesdeUtc, DateTime HastaUtc) DiaCalendarioAUtc(DateTime? desde, DateTime? hasta) =>
        (InicioDiaAUtc(desde ?? DateTime.Today)!.Value, FinDiaAUtc(hasta ?? DateTime.Today)!.Value);

    private static DateTime ConvertirAUtc(DateTime fechaLocalSinHora) =>
        TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(fechaLocalSinHora, DateTimeKind.Unspecified), TimeZoneInfo.Local);
}
