namespace Inventario.Web.Services.Sesion;

/// <summary>
/// Nombres de los claims propios que se guardan en la cookie de sesión (además de los estándar:
/// ClaimTypes.NameIdentifier, ClaimTypes.Name y ClaimTypes.Role, que sí se reutilizan tal cual para
/// poder usar [Authorize(Roles = "...")] / User.IsInRole(...) sin traducción).
/// </summary>
internal static class InventarioClaimTypes
{
    /// <summary>JWT emitido por Inventario.Api en el login; se reenvía como Bearer en cada llamada
    /// a la Api (ver AuthHeaderHandler).</summary>
    public const string Token = "inv_token";

    public const string NombreCompleto = "inv_nombre_completo";

    /// <summary>Inventario con el que está operando la sesión ahora mismo. Ausente hasta que el
    /// usuario lo elige (login con un solo Inventario lo fija automático).</summary>
    public const string InventarioId = "inv_inventario_id";

    /// <summary>JSON de InventarioDto[] con los Inventarios a los que el usuario tiene acceso (se
    /// calcula una sola vez en el login; no cambia hasta volver a iniciar sesión).</summary>
    public const string InventariosDisponibles = "inv_inventarios_disponibles";
}
