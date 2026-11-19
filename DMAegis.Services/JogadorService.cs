using System.Security.Cryptography;
using System.Text;
using DMAegis.Model;
using DMAegis.Repository;
using DMAegis.Services.Exceptions;

namespace DMAegis.Services;

public class ConviteServiceModel
{
    public int IdRegistro { get; set; }
    public string NomePersonagem { get; set; } = string.Empty;
    public bool TemConvite { get; set; }
    public DateTime? DataUltimoAcesso { get; set; }
}

/// <summary>Quem está do outro lado do link: a campanha, o PJ e a marca do convite usado.</summary>
public class JogadorLogadoServiceModel
{
    public int IdCampanha { get; set; }
    public int IdRegistro { get; set; }
    public string NomePersonagem { get; set; } = string.Empty;
    public string NomeCampanha { get; set; } = string.Empty;

    /// <summary>Começo do hash do convite: se o mestre trocar o link, a marca deixa de bater e o acesso cai.</summary>
    public string Marca { get; set; } = string.Empty;
}

public interface IJogadorService
{
    // Lado do mestre
    Task<IEnumerable<ConviteServiceModel>> ListarConvitesAsync(int idUsuario, int idCampanha);

    /// <summary>Gera (ou troca) o link do PJ. O código só existe nesta resposta: o banco guarda o hash.</summary>
    Task<string> GerarConviteAsync(int idUsuario, int idRegistro);
    Task RevogarConviteAsync(int idUsuario, int idRegistro);

    // Lado do jogador
    Task<JogadorLogadoServiceModel> EntrarAsync(string codigo);

    /// <summary>Confere se o link usado continua valendo. Se o mestre trocou, AcessoJogadorExpirado (401).</summary>
    Task ConferirAcessoAsync(JogadorLogadoServiceModel jogador);

    /// <summary>A mesa ativa da campanha, já filtrada para o jogador. Nulo = nenhuma mesa ativa.</summary>
    Task<VisaoJogadorServiceModel?> ObterMesaAsync(JogadorLogadoServiceModel jogador);
    Task MoverMinhaFichaAsync(JogadorLogadoServiceModel jogador, int coluna, int linha);

    /// <summary>Só o mapa da mesa ativa e os retratos das fichas que o jogador vê.</summary>
    Task<(string TipoConteudo, byte[] Conteudo)> ObterImagemAsync(JogadorLogadoServiceModel jogador, int idImagem);
}

public class JogadorService : IJogadorService
{
    public const int TamanhoMarca = 16;

    private readonly IConviteJogadorRepository _conviteRepository;
    private readonly IRegistroRepository _registroRepository;
    private readonly ICampanhaRepository _campanhaRepository;
    private readonly IParticipanteMesaRepository _participanteRepository;
    private readonly IImagemRegistroRepository _imagemRepository;
    private readonly IRegistroService _registroService;
    private readonly ICampanhaService _campanhaService;
    private readonly IMesaService _mesaService;
    private readonly ICanalMesa _canal;

    public JogadorService(
        IConviteJogadorRepository conviteRepository,
        IRegistroRepository registroRepository,
        ICampanhaRepository campanhaRepository,
        IParticipanteMesaRepository participanteRepository,
        IImagemRegistroRepository imagemRepository,
        IRegistroService registroService,
        ICampanhaService campanhaService,
        IMesaService mesaService,
        ICanalMesa canal)
    {
        _conviteRepository = conviteRepository;
        _registroRepository = registroRepository;
        _campanhaRepository = campanhaRepository;
        _participanteRepository = participanteRepository;
        _imagemRepository = imagemRepository;
        _registroService = registroService;
        _campanhaService = campanhaService;
        _mesaService = mesaService;
        _canal = canal;
    }

    /// <summary>SHA-256 do código em hexadecimal minúsculo (64 caracteres).</summary>
    public static string HashDoCodigo(string codigo) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(codigo.Trim())));

    /// <summary>32 bytes aleatórios em Base64Url (43 caracteres, seguros para URL).</summary>
    public static string NovoCodigo() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    // ---------- Lado do mestre ----------

    public async Task<IEnumerable<ConviteServiceModel>> ListarConvitesAsync(int idUsuario, int idCampanha)
    {
        await _campanhaService.ObterDoUsuarioOuLancarAsync(idUsuario, idCampanha);
        var jogadores = await _registroRepository.ListarAsync(idCampanha, new FiltroRegistro { IdTipoRegistro = MesaService.IdTipoJogador });
        var convites = (await _conviteRepository.ListarPorCampanhaAsync(idCampanha)).ToDictionary(c => c.IdRegistro);
        return jogadores.Select(r => new ConviteServiceModel
        {
            IdRegistro = r.Id,
            NomePersonagem = r.Nome,
            TemConvite = convites.ContainsKey(r.Id),
            DataUltimoAcesso = convites.TryGetValue(r.Id, out var c) ? c.DataUltimoAcesso : null,
        }).ToList();
    }

    public async Task<string> GerarConviteAsync(int idUsuario, int idRegistro)
    {
        var registro = await ObterJogadorDoUsuarioOuLancarAsync(idUsuario, idRegistro);
        var codigo = NovoCodigo();
        await _conviteRepository.SalvarAsync(new ConviteJogador
        {
            IdCampanha = registro.IdCampanha,
            IdRegistro = registro.Id,
            HashCodigo = HashDoCodigo(codigo),
            DataCriacao = Agora(),
        });
        // Quem estava com o link antigo cai na próxima atualização.
        _canal.Avisar(registro.IdCampanha);
        return codigo;
    }

    public async Task RevogarConviteAsync(int idUsuario, int idRegistro)
    {
        var registro = await ObterJogadorDoUsuarioOuLancarAsync(idUsuario, idRegistro);
        await _conviteRepository.ExcluirPorRegistroAsync(registro.Id);
        _canal.Avisar(registro.IdCampanha);
    }

    // ---------- Lado do jogador ----------

    public async Task<JogadorLogadoServiceModel> EntrarAsync(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo) || codigo.Length > 100)
            throw new BusinessRuleException(BusinessErrors.ConviteInvalido);

        var convite = await _conviteRepository.ObterPorHashAsync(HashDoCodigo(codigo))
                      ?? throw new BusinessRuleException(BusinessErrors.ConviteInvalido);
        var registro = await _registroRepository.ObterAsync(convite.IdRegistro)
                       ?? throw new BusinessRuleException(BusinessErrors.ConviteInvalido);
        var campanha = await _campanhaRepository.ObterAsync(convite.IdCampanha)
                       ?? throw new BusinessRuleException(BusinessErrors.ConviteInvalido);

        await _conviteRepository.RegistrarAcessoAsync(convite.Id, Agora());
        return new JogadorLogadoServiceModel
        {
            IdCampanha = convite.IdCampanha,
            IdRegistro = convite.IdRegistro,
            NomePersonagem = registro.Nome,
            NomeCampanha = campanha.Nome,
            Marca = convite.HashCodigo[..TamanhoMarca],
        };
    }

    public async Task ConferirAcessoAsync(JogadorLogadoServiceModel jogador)
    {
        var convite = await _conviteRepository.ObterPorRegistroAsync(jogador.IdRegistro);
        if (convite is null || convite.IdCampanha != jogador.IdCampanha
            || !CryptographicOperations.FixedTimeEquals(
                Encoding.ASCII.GetBytes(convite.HashCodigo[..TamanhoMarca]), Encoding.ASCII.GetBytes(jogador.Marca.PadRight(TamanhoMarca)[..TamanhoMarca])))
            throw new BusinessRuleException(BusinessErrors.AcessoJogadorExpirado);
    }

    public async Task<VisaoJogadorServiceModel?> ObterMesaAsync(JogadorLogadoServiceModel jogador)
    {
        await ConferirAcessoAsync(jogador);
        var mesa = await _mesaService.MontarAtivaAsync(jogador.IdCampanha);
        return mesa is null ? null : MesaService.FiltrarParaJogador(mesa, jogador.IdRegistro);
    }

    public async Task MoverMinhaFichaAsync(JogadorLogadoServiceModel jogador, int coluna, int linha)
    {
        await ConferirAcessoAsync(jogador);
        var mesa = await _mesaService.MontarAtivaAsync(jogador.IdCampanha)
                   ?? throw new BusinessRuleException(BusinessErrors.FichaForaDaMesa);
        if (!mesa.MovimentoLivre)
            throw new BusinessRuleException(BusinessErrors.MovimentoBloqueado);

        // Só a própria ficha, visível e já no mapa (quem coloca no mapa é o mestre).
        var minha = mesa.Participantes.FirstOrDefault(p => p.IdRegistro == jogador.IdRegistro && p.Visivel && p.Coluna.HasValue)
                    ?? throw new BusinessRuleException(BusinessErrors.FichaForaDaMesa);
        await _participanteRepository.AtualizarPosicaoAsync(minha.Id, coluna, linha);
        _canal.Avisar(jogador.IdCampanha);
    }

    public async Task<(string TipoConteudo, byte[] Conteudo)> ObterImagemAsync(JogadorLogadoServiceModel jogador, int idImagem)
    {
        var visao = await ObterMesaAsync(jogador);
        var permitidas = new HashSet<int>();
        if (visao?.IdImagemRegistro is int mapa) permitidas.Add(mapa);
        foreach (var p in visao?.Participantes ?? [])
            if (p.IdImagemRetrato is int retrato) permitidas.Add(retrato);

        if (!permitidas.Contains(idImagem))
            throw new BusinessRuleException(BusinessErrors.ImagemNaoEncontrada);
        return await _imagemRepository.ObterConteudoAsync(idImagem)
               ?? throw new BusinessRuleException(BusinessErrors.ImagemNaoEncontrada);
    }

    // ---------- Apoio ----------

    private async Task<RegistroServiceModel> ObterJogadorDoUsuarioOuLancarAsync(int idUsuario, int idRegistro)
    {
        // O RegistroService já responde RegistroNaoEncontrado (404) para registro de outro usuário.
        var registro = await _registroService.ObterAsync(idUsuario, idRegistro);
        if (registro.IdTipoRegistro != MesaService.IdTipoJogador)
            throw new BusinessRuleException(BusinessErrors.SoJogadorTemConvite);
        return registro;
    }

    private static DateTime Agora()
    {
        var agora = DateTime.Now;
        return new DateTime(agora.Year, agora.Month, agora.Day, agora.Hour, agora.Minute, agora.Second, DateTimeKind.Unspecified);
    }
}
