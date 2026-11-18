namespace DMAegis.Services.Exceptions;

/// <summary>
/// Erros de negócio: (código, mensagem padrão). Códigos sequenciais a partir de 2
/// (0 = sucesso e 1 = erro genérico, ver ResponseCodes). Código novo = próximo número livre.
/// O status HTTP de cada código fica em DMAegis.WebApi/Middlewares/StatusErroNegocio.cs.
/// </summary>
public static class BusinessErrors
{
    public static readonly (int Code, string DefaultMessage) DadosInvalidos = (2, "Dados inválidos.");
    public static readonly (int Code, string DefaultMessage) EmailJaCadastrado = (3, "Este e-mail já está cadastrado.");
    public static readonly (int Code, string DefaultMessage) CredenciaisInvalidas = (4, "E-mail ou senha inválidos.");
    public static readonly (int Code, string DefaultMessage) CampanhaNaoEncontrada = (5, "Campanha não encontrada.");
    public static readonly (int Code, string DefaultMessage) SistemaInvalido = (6, "Sistema inválido.");
    public static readonly (int Code, string DefaultMessage) RegistroNaoEncontrado = (7, "Registro não encontrado.");
    public static readonly (int Code, string DefaultMessage) TipoRegistroInvalido = (8, "Tipo de registro inválido.");
    public static readonly (int Code, string DefaultMessage) SessaoNumeroDuplicado = (9, "Já existe uma sessão com esse número.");
    public static readonly (int Code, string DefaultMessage) SessaoNaoEncontrada = (10, "Sessão não encontrada.");
    public static readonly (int Code, string DefaultMessage) SegredoNaoEncontrado = (11, "Segredo não encontrado.");
}
