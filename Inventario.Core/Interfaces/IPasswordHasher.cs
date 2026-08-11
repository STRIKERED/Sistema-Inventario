namespace Inventario.Core.Interfaces;

public interface IPasswordHasher
{
    /// <summary>Genera un hash seguro (con salt embebido) para almacenar en <c>Usuario.PasswordHash</c>.</summary>
    string Hashear(string password);

    /// <summary>Verifica que <paramref name="password"/> corresponda al hash previamente generado por <see cref="Hashear"/>.</summary>
    bool Verificar(string password, string hashAlmacenado);
}
