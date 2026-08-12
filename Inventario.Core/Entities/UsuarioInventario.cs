namespace Inventario.Core.Entities;

/// <summary>Tabla puente: qué Inventarios puede operar cada Usuario. Llave compuesta (UsuarioId, InventarioId).
/// Administrador es la única excepción: tiene acceso implícito a todos los Inventarios activos sin
/// necesidad de una fila aquí (ver AuthController).</summary>
public class UsuarioInventario
{
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public int InventarioId { get; set; }
    public Inventario? Inventario { get; set; }
}
