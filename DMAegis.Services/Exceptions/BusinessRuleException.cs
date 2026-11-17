namespace DMAegis.Services.Exceptions;

/// <summary>
/// Erro de regra de negócio. O GlobalExceptionHandler responde 400 com o código e a mensagem.
/// </summary>
public class BusinessRuleException : Exception
{
    public int Code { get; }
    public string Mensagem { get; }

    public BusinessRuleException((int Code, string DefaultMessage) erro)
        : base(erro.DefaultMessage)
    {
        Code = erro.Code;
        Mensagem = erro.DefaultMessage;
    }
}
