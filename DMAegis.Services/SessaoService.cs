using DMAegis.Model;
using DMAegis.Repository;
using DMAegis.Services.Exceptions;

namespace DMAegis.Services;

public class SessaoResumoServiceModel
{
    public int Id { get; set; }
    public int Numero { get; set; }
    public DateTime? Data { get; set; }
    public string? ComecoForte { get; set; }
}

public class SegredoServiceModel
{
    public int Id { get; set; }
    public string Texto { get; set; } = string.Empty;
    public int? IdSessaoCriacao { get; set; }
    public int? IdSessaoRevelacao { get; set; }
    public DateTime? DataRevelacao { get; set; }
}

public class SessaoServiceModel
{
    public int Id { get; set; }
    public int IdCampanha { get; set; }
    public int Numero { get; set; }
    public DateTime? Data { get; set; }
    public string? NotasPersonagens { get; set; }
    public string? ComecoForte { get; set; }
    public string? Resumo { get; set; }
    public List<string> Cenas { get; set; } = [];
    public List<int> IdsRegistro { get; set; } = [];

    /// <summary>Pendentes da campanha + revelados nesta sessão.</summary>
    public List<SegredoServiceModel> Segredos { get; set; } = [];
}

public class AnotacaoServiceModel
{
    public int Id { get; set; }
    public DateTime Data { get; set; }
    public string Texto { get; set; } = string.Empty;
}

public class InserirSessaoServiceModel
{
    /// <summary>Nulo = o maior número da campanha + 1.</summary>
    public int? Numero { get; set; }
    public DateTime? Data { get; set; }
}

public class SalvarSessaoServiceModel
{
    public int Numero { get; set; }
    public DateTime? Data { get; set; }
    public string? NotasPersonagens { get; set; }
    public string? ComecoForte { get; set; }
    public string? Resumo { get; set; }
    public List<string> Cenas { get; set; } = [];
    public List<int> IdsRegistro { get; set; } = [];
}

public interface ISessaoService
{
    Task<IEnumerable<SessaoResumoServiceModel>> ListarAsync(int idUsuario, int idCampanha);
    Task<SessaoServiceModel> ObterAsync(int idUsuario, int id);
    Task<SessaoServiceModel> InserirAsync(int idUsuario, int idCampanha, InserirSessaoServiceModel dados);
    Task<SessaoServiceModel> AtualizarAsync(int idUsuario, int id, SalvarSessaoServiceModel dados);
    Task ExcluirAsync(int idUsuario, int id);

    Task<SegredoServiceModel> InserirSegredoAsync(int idUsuario, int idSessao, string texto);
    Task<SegredoServiceModel> AtualizarSegredoAsync(int idUsuario, int idSegredo, string texto);
    Task<SegredoServiceModel> RevelarSegredoAsync(int idUsuario, int idSegredo, int idSessao);
    Task<SegredoServiceModel> OcultarSegredoAsync(int idUsuario, int idSegredo);
    Task ExcluirSegredoAsync(int idUsuario, int idSegredo);

    Task<IEnumerable<AnotacaoServiceModel>> ListarAnotacoesAsync(int idUsuario, int idSessao);

    /// <summary>Grava a anotação com o horário do servidor.</summary>
    Task<AnotacaoServiceModel> InserirAnotacaoAsync(int idUsuario, int idSessao, string texto);

    Task ExcluirAnotacaoAsync(int idUsuario, int idAnotacao);
}

public class SessaoService : ISessaoService
{
    private readonly ISessaoRepository _sessaoRepository;
    private readonly ICenaSessaoRepository _cenaRepository;
    private readonly ISessaoRegistroRepository _vinculoRepository;
    private readonly ISegredoRepository _segredoRepository;
    private readonly IRegistroRepository _registroRepository;
    private readonly ICampanhaService _campanhaService;
    private readonly IAnotacaoSessaoRepository _anotacaoRepository;

    public SessaoService(
        ISessaoRepository sessaoRepository,
        ICenaSessaoRepository cenaRepository,
        ISessaoRegistroRepository vinculoRepository,
        ISegredoRepository segredoRepository,
        IRegistroRepository registroRepository,
        ICampanhaService campanhaService,
        IAnotacaoSessaoRepository anotacaoRepository)
    {
        _sessaoRepository = sessaoRepository;
        _cenaRepository = cenaRepository;
        _vinculoRepository = vinculoRepository;
        _segredoRepository = segredoRepository;
        _registroRepository = registroRepository;
        _campanhaService = campanhaService;
        _anotacaoRepository = anotacaoRepository;
    }

    public async Task<IEnumerable<AnotacaoServiceModel>> ListarAnotacoesAsync(int idUsuario, int idSessao)
    {
        await ObterSessaoDoUsuarioOuLancarAsync(idUsuario, idSessao);
        return (await _anotacaoRepository.ListarPorSessaoAsync(idSessao)).Select(ParaServiceModel).ToList();
    }

    public async Task<AnotacaoServiceModel> InserirAnotacaoAsync(int idUsuario, int idSessao, string texto)
    {
        await ObterSessaoDoUsuarioOuLancarAsync(idUsuario, idSessao);
        // Corta no segundo e sem fuso, igual ao que o DATETIME do MySQL devolve depois.
        var agora = DateTime.Now;
        var data = new DateTime(agora.Year, agora.Month, agora.Day, agora.Hour, agora.Minute, agora.Second, DateTimeKind.Unspecified);
        var anotacao = new AnotacaoSessao { IdSessao = idSessao, Data = data, Texto = texto.Trim() };
        anotacao.Id = await _anotacaoRepository.InserirAsync(anotacao);
        return ParaServiceModel(anotacao);
    }

    public async Task ExcluirAnotacaoAsync(int idUsuario, int idAnotacao)
    {
        var anotacao = await _anotacaoRepository.ObterAsync(idAnotacao)
                       ?? throw new BusinessRuleException(BusinessErrors.AnotacaoNaoEncontrada);
        try
        {
            await ObterSessaoDoUsuarioOuLancarAsync(idUsuario, anotacao.IdSessao);
        }
        catch (BusinessRuleException e) when (e.Code == BusinessErrors.SessaoNaoEncontrada.Code)
        {
            throw new BusinessRuleException(BusinessErrors.AnotacaoNaoEncontrada);
        }
        await _anotacaoRepository.ExcluirAsync(idAnotacao);
    }

    private static AnotacaoServiceModel ParaServiceModel(AnotacaoSessao anotacao) => new()
    {
        Id = anotacao.Id,
        Data = anotacao.Data,
        Texto = anotacao.Texto,
    };

    public async Task<IEnumerable<SessaoResumoServiceModel>> ListarAsync(int idUsuario, int idCampanha)
    {
        await _campanhaService.ObterDoUsuarioOuLancarAsync(idUsuario, idCampanha);
        return (await _sessaoRepository.ListarPorCampanhaAsync(idCampanha))
            .Select(s => new SessaoResumoServiceModel { Id = s.Id, Numero = s.Numero, Data = s.Data, ComecoForte = s.ComecoForte })
            .ToList();
    }

    public async Task<SessaoServiceModel> ObterAsync(int idUsuario, int id) =>
        await MontarAsync(await ObterSessaoDoUsuarioOuLancarAsync(idUsuario, id));

    public async Task<SessaoServiceModel> InserirAsync(int idUsuario, int idCampanha, InserirSessaoServiceModel dados)
    {
        await _campanhaService.ObterDoUsuarioOuLancarAsync(idUsuario, idCampanha);

        var sessao = new Sessao
        {
            IdCampanha = idCampanha,
            Numero = dados.Numero ?? await _sessaoRepository.ObterMaiorNumeroAsync(idCampanha) + 1,
            Data = dados.Data?.Date,
        };
        try
        {
            sessao.Id = await _sessaoRepository.InserirAsync(sessao);
        }
        catch (RegistroDuplicadoException)
        {
            throw new BusinessRuleException(BusinessErrors.SessaoNumeroDuplicado);
        }

        return await MontarAsync(sessao);
    }

    public async Task<SessaoServiceModel> AtualizarAsync(int idUsuario, int id, SalvarSessaoServiceModel dados)
    {
        var sessao = await ObterSessaoDoUsuarioOuLancarAsync(idUsuario, id);

        var idsRegistro = dados.IdsRegistro.Distinct().ToList();
        if (idsRegistro.Count > 0)
        {
            var daCampanha = (await _registroRepository.ListarAsync(sessao.IdCampanha, new FiltroRegistro()))
                .Select(r => r.Id).ToHashSet();
            if (idsRegistro.Any(idRegistro => !daCampanha.Contains(idRegistro)))
                throw new BusinessRuleException(BusinessErrors.RegistroNaoEncontrado);
        }

        sessao.Numero = dados.Numero;
        sessao.Data = dados.Data?.Date;
        sessao.NotasPersonagens = TextoOuNulo(dados.NotasPersonagens);
        sessao.ComecoForte = TextoOuNulo(dados.ComecoForte);
        sessao.Resumo = TextoOuNulo(dados.Resumo);

        var cenas = dados.Cenas
            .Select(c => (c ?? string.Empty).Trim())
            .Where(c => c.Length > 0)
            .Select((descricao, indice) => new CenaSessao { IdSessao = id, Descricao = descricao, Ordem = indice + 1 })
            .ToList();

        try
        {
            using var transacao = Transacao.Iniciar();
            await _sessaoRepository.AtualizarAsync(sessao);
            await _cenaRepository.SubstituirAsync(id, cenas);
            await _vinculoRepository.SubstituirAsync(id, idsRegistro);
            transacao.Complete();
        }
        catch (RegistroDuplicadoException)
        {
            throw new BusinessRuleException(BusinessErrors.SessaoNumeroDuplicado);
        }

        return await MontarAsync(sessao);
    }

    public async Task ExcluirAsync(int idUsuario, int id)
    {
        await ObterSessaoDoUsuarioOuLancarAsync(idUsuario, id);
        await _sessaoRepository.ExcluirAsync(id);
    }

    public async Task<SegredoServiceModel> InserirSegredoAsync(int idUsuario, int idSessao, string texto)
    {
        var sessao = await ObterSessaoDoUsuarioOuLancarAsync(idUsuario, idSessao);
        var segredo = new Segredo { IdCampanha = sessao.IdCampanha, IdSessaoCriacao = sessao.Id, Texto = texto.Trim() };
        segredo.Id = await _segredoRepository.InserirAsync(segredo);
        return ParaServiceModel(segredo);
    }

    public async Task<SegredoServiceModel> AtualizarSegredoAsync(int idUsuario, int idSegredo, string texto)
    {
        var segredo = await ObterSegredoDoUsuarioOuLancarAsync(idUsuario, idSegredo);
        segredo.Texto = texto.Trim();
        await _segredoRepository.AtualizarAsync(segredo);
        return ParaServiceModel(segredo);
    }

    public async Task<SegredoServiceModel> RevelarSegredoAsync(int idUsuario, int idSegredo, int idSessao)
    {
        var segredo = await ObterSegredoDoUsuarioOuLancarAsync(idUsuario, idSegredo);
        var sessao = await _sessaoRepository.ObterAsync(idSessao);
        if (sessao is null || sessao.IdCampanha != segredo.IdCampanha)
            throw new BusinessRuleException(BusinessErrors.SessaoNaoEncontrada);

        segredo.IdSessaoRevelacao = idSessao;
        segredo.DataRevelacao = DateTime.Now;
        await _segredoRepository.AtualizarAsync(segredo);
        return ParaServiceModel(segredo);
    }

    public async Task<SegredoServiceModel> OcultarSegredoAsync(int idUsuario, int idSegredo)
    {
        var segredo = await ObterSegredoDoUsuarioOuLancarAsync(idUsuario, idSegredo);
        segredo.IdSessaoRevelacao = null;
        segredo.DataRevelacao = null;
        await _segredoRepository.AtualizarAsync(segredo);
        return ParaServiceModel(segredo);
    }

    public async Task ExcluirSegredoAsync(int idUsuario, int idSegredo)
    {
        await ObterSegredoDoUsuarioOuLancarAsync(idUsuario, idSegredo);
        await _segredoRepository.ExcluirAsync(idSegredo);
    }

    /// <summary>Sessão inexistente ou de campanha de outro usuário → SessaoNaoEncontrada (404).</summary>
    private async Task<Sessao> ObterSessaoDoUsuarioOuLancarAsync(int idUsuario, int id)
    {
        var sessao = await _sessaoRepository.ObterAsync(id)
                     ?? throw new BusinessRuleException(BusinessErrors.SessaoNaoEncontrada);
        await ConferirCampanhaAsync(idUsuario, sessao.IdCampanha, BusinessErrors.SessaoNaoEncontrada);
        return sessao;
    }

    /// <summary>Segredo inexistente ou de campanha de outro usuário → SegredoNaoEncontrado (404).</summary>
    private async Task<Segredo> ObterSegredoDoUsuarioOuLancarAsync(int idUsuario, int id)
    {
        var segredo = await _segredoRepository.ObterAsync(id)
                      ?? throw new BusinessRuleException(BusinessErrors.SegredoNaoEncontrado);
        await ConferirCampanhaAsync(idUsuario, segredo.IdCampanha, BusinessErrors.SegredoNaoEncontrado);
        return segredo;
    }

    private async Task ConferirCampanhaAsync(int idUsuario, int idCampanha, (int Code, string DefaultMessage) erroSeAlheia)
    {
        try
        {
            await _campanhaService.ObterDoUsuarioOuLancarAsync(idUsuario, idCampanha);
        }
        catch (BusinessRuleException e) when (e.Code == BusinessErrors.CampanhaNaoEncontrada.Code)
        {
            throw new BusinessRuleException(erroSeAlheia);
        }
    }

    private async Task<SessaoServiceModel> MontarAsync(Sessao sessao)
    {
        var cenas = await _cenaRepository.ListarPorSessaoAsync(sessao.Id);
        var idsRegistro = await _vinculoRepository.ListarIdsRegistroAsync(sessao.Id);
        var segredos = await _segredoRepository.ListarDaSessaoAsync(sessao.IdCampanha, sessao.Id);

        return new SessaoServiceModel
        {
            Id = sessao.Id,
            IdCampanha = sessao.IdCampanha,
            Numero = sessao.Numero,
            Data = sessao.Data,
            NotasPersonagens = sessao.NotasPersonagens,
            ComecoForte = sessao.ComecoForte,
            Resumo = sessao.Resumo,
            Cenas = cenas.OrderBy(c => c.Ordem).Select(c => c.Descricao).ToList(),
            IdsRegistro = idsRegistro.ToList(),
            Segredos = segredos.Select(ParaServiceModel).ToList(),
        };
    }

    private static SegredoServiceModel ParaServiceModel(Segredo segredo) => new()
    {
        Id = segredo.Id,
        Texto = segredo.Texto,
        IdSessaoCriacao = segredo.IdSessaoCriacao,
        IdSessaoRevelacao = segredo.IdSessaoRevelacao,
        DataRevelacao = segredo.DataRevelacao,
    };

    private static string? TextoOuNulo(string? texto) =>
        string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
}
