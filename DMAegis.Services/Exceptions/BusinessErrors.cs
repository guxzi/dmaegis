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
    public static readonly (int Code, string DefaultMessage) AnotacaoNaoEncontrada = (12, "Anotação não encontrada.");
    public static readonly (int Code, string DefaultMessage) ImagemNaoEncontrada = (13, "Imagem não encontrada.");
    public static readonly (int Code, string DefaultMessage) ImagemInvalida = (14, "Envie uma imagem JPEG, PNG, WebP ou GIF.");
    public static readonly (int Code, string DefaultMessage) ImagemGrandeDemais = (15, "A imagem pode ter até 15 MB.");
    public static readonly (int Code, string DefaultMessage) LimiteImagens = (16, "Um registro pode ter até 30 imagens.");
    public static readonly (int Code, string DefaultMessage) MesaNaoEncontrada = (17, "Mesa não encontrada.");
    public static readonly (int Code, string DefaultMessage) ParticipanteNaoEncontrado = (18, "Ficha não encontrada.");
    public static readonly (int Code, string DefaultMessage) LimiteParticipantes = (19, "Uma mesa pode ter até 60 fichas.");
    public static readonly (int Code, string DefaultMessage) ConviteInvalido = (20, "Este link de convite não vale mais. Peça o novo ao mestre.");
    public static readonly (int Code, string DefaultMessage) AcessoJogadorExpirado = (21, "O mestre trocou o seu link. Peça o novo para voltar à mesa.");
    public static readonly (int Code, string DefaultMessage) MovimentoBloqueado = (22, "O mestre travou o movimento das fichas.");
    public static readonly (int Code, string DefaultMessage) FichaForaDaMesa = (23, "Sua ficha não está no mapa agora.");
    public static readonly (int Code, string DefaultMessage) SoJogadorTemConvite = (24, "Só personagens de jogador (PJ) recebem link.");
    public static readonly (int Code, string DefaultMessage) EntregaNaoEncontrada = (25, "Entrega não encontrada.");
    public static readonly (int Code, string DefaultMessage) IniciativaJaDefinida = (26, "Sua iniciativa já está na ordem do combate.");
}
