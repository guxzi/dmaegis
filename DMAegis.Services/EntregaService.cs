using DMAegis.Model;
using DMAegis.Repository;
using DMAegis.Services.Exceptions;

namespace DMAegis.Services;

public class EntregaServiceModel
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Texto { get; set; }
    public int? IdImagemRegistro { get; set; }
    public int? IdRegistroDestino { get; set; }

    /// <summary>Nome do PJ de destino (nulo = todos). Só para o mestre.</summary>
    public string? NomeDestino { get; set; }

    public DateTime Data { get; set; }
}

public class InserirEntregaServiceModel
{
    public string Titulo { get; set; } = string.Empty;
    public string? Texto { get; set; }
    public int? IdImagemRegistro { get; set; }
    public int? IdRegistroDestino { get; set; }
}

public interface IEntregaService
{
    Task<IEnumerable<EntregaServiceModel>> ListarAsync(int idUsuario, int idCampanha);
    Task<EntregaServiceModel> InserirAsync(int idUsuario, int idCampanha, InserirEntregaServiceModel dados);
    Task ExcluirAsync(int idUsuario, int id);

    /// <summary>As entregas que o jogador pode ver (para todos e só dele).</summary>
    Task<IEnumerable<EntregaServiceModel>> ListarDoJogadorAsync(JogadorLogadoServiceModel jogador);
}

/// <summary>Entregas aos jogadores (handouts). O mestre confere o dono da campanha; o jogador só vê as dele.</summary>
public class EntregaService : IEntregaService
{
    private const int TipoPersonagemJogador = 2;

    private readonly IEntregaRepository _entregaRepository;
    private readonly IImagemRegistroRepository _imagemRepository;
    private readonly IRegistroRepository _registroRepository;
    private readonly ICampanhaService _campanhaService;
    private readonly IJogadorService _jogadorService;
    private readonly ICanalMesa _canal;

    public EntregaService(IEntregaRepository entregaRepository, IImagemRegistroRepository imagemRepository,
        IRegistroRepository registroRepository, ICampanhaService campanhaService, IJogadorService jogadorService, ICanalMesa canal)
    {
        _entregaRepository = entregaRepository;
        _imagemRepository = imagemRepository;
        _registroRepository = registroRepository;
        _campanhaService = campanhaService;
        _jogadorService = jogadorService;
        _canal = canal;
    }

    public async Task<IEnumerable<EntregaServiceModel>> ListarAsync(int idUsuario, int idCampanha)
    {
        await _campanhaService.ObterDoUsuarioOuLancarAsync(idUsuario, idCampanha);
        var entregas = (await _entregaRepository.ListarPorCampanhaAsync(idCampanha)).ToList();
        var nomes = new Dictionary<int, string>();
        foreach (var id in entregas.Where(e => e.IdRegistroDestino.HasValue).Select(e => e.IdRegistroDestino!.Value).Distinct())
            if (await _registroRepository.ObterAsync(id) is { } registro)
                nomes[id] = registro.Nome;
        return entregas.Select(e => ParaServiceModel(e, e.IdRegistroDestino is int d && nomes.TryGetValue(d, out var nome) ? nome : null)).ToList();
    }

    public async Task<EntregaServiceModel> InserirAsync(int idUsuario, int idCampanha, InserirEntregaServiceModel dados)
    {
        await _campanhaService.ObterDoUsuarioOuLancarAsync(idUsuario, idCampanha);

        var texto = string.IsNullOrWhiteSpace(dados.Texto) ? null : dados.Texto.Trim();
        if (texto is null && dados.IdImagemRegistro is null)
            throw new BusinessRuleException((BusinessErrors.DadosInvalidos.Code, "Escreva um texto ou escolha uma imagem."));

        // A imagem precisa ser da galeria desta campanha.
        if (dados.IdImagemRegistro is int idImagem
            && !(await _imagemRepository.ListarPorCampanhaAsync(idCampanha)).Any(i => i.Id == idImagem))
            throw new BusinessRuleException(BusinessErrors.ImagemNaoEncontrada);

        // O destino, se houver, é um PJ desta campanha.
        string? nomeDestino = null;
        if (dados.IdRegistroDestino is int idDestino)
        {
            var registro = await _registroRepository.ObterAsync(idDestino);
            if (registro is null || registro.IdCampanha != idCampanha)
                throw new BusinessRuleException(BusinessErrors.RegistroNaoEncontrado);
            if (registro.IdTipoRegistro != TipoPersonagemJogador)
                throw new BusinessRuleException(BusinessErrors.SoJogadorTemConvite);
            nomeDestino = registro.Nome;
        }

        var agora = DateTime.Now;
        var entrega = new Entrega
        {
            IdCampanha = idCampanha,
            Titulo = dados.Titulo.Trim(),
            Texto = texto,
            IdImagemRegistro = dados.IdImagemRegistro,
            IdRegistroDestino = dados.IdRegistroDestino,
            Data = new DateTime(agora.Year, agora.Month, agora.Day, agora.Hour, agora.Minute, agora.Second, DateTimeKind.Unspecified),
        };
        entrega.Id = await _entregaRepository.InserirAsync(entrega);
        _canal.Avisar(idCampanha);
        return ParaServiceModel(entrega, nomeDestino);
    }

    public async Task ExcluirAsync(int idUsuario, int id)
    {
        var entrega = await _entregaRepository.ObterAsync(id) ?? throw new BusinessRuleException(BusinessErrors.EntregaNaoEncontrada);
        try
        {
            await _campanhaService.ObterDoUsuarioOuLancarAsync(idUsuario, entrega.IdCampanha);
        }
        catch (BusinessRuleException e) when (e.Code == BusinessErrors.CampanhaNaoEncontrada.Code)
        {
            throw new BusinessRuleException(BusinessErrors.EntregaNaoEncontrada);
        }
        await _entregaRepository.ExcluirAsync(id);
        _canal.Avisar(entrega.IdCampanha);
    }

    public async Task<IEnumerable<EntregaServiceModel>> ListarDoJogadorAsync(JogadorLogadoServiceModel jogador)
    {
        await _jogadorService.ConferirAcessoAsync(jogador);
        return (await _entregaRepository.ListarDoJogadorAsync(jogador.IdCampanha, jogador.IdRegistro))
            .Select(e => ParaServiceModel(e, null))
            .ToList();
    }

    private static EntregaServiceModel ParaServiceModel(Entrega e, string? nomeDestino) => new()
    {
        Id = e.Id,
        Titulo = e.Titulo,
        Texto = e.Texto,
        IdImagemRegistro = e.IdImagemRegistro,
        IdRegistroDestino = e.IdRegistroDestino,
        NomeDestino = nomeDestino,
        Data = e.Data,
    };
}
