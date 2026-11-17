namespace DMAegis.Repository;

/// <summary>
/// Lançada quando o MySQL recusa um INSERT/UPDATE por valor duplicado (erro 1062).
/// O Service converte no erro de negócio correspondente.
/// </summary>
public class RegistroDuplicadoException : Exception
{
    public RegistroDuplicadoException() : base("Registro duplicado.") { }

    public RegistroDuplicadoException(Exception inner) : base("Registro duplicado.", inner) { }
}
