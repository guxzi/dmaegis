using System.Globalization;
using DMAegis.Model;
using DMAegis.Repository;
using DMAegis.Services.Exceptions;

namespace DMAegis.Services;

public class TipoRegistroServiceModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}

public class ModeloAtributoServiceModel
{
    public int IdTipoRegistro { get; set; }
    public string Chave { get; set; } = string.Empty;
    public int Ordem { get; set; }
}

public class TagServiceModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}

public class AtributoServiceModel
{
    public string Chave { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
}

public class RegistroResumoServiceModel
{
    public int Id { get; set; }
    public int IdTipoRegistro { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Resumo { get; set; }
    public List<TagServiceModel> Tags { get; set; } = [];
}

public class RegistroServiceModel
{
    public int Id { get; set; }
    public int IdCampanha { get; set; }
    public int IdTipoRegistro { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Resumo { get; set; }
    public string? Descricao { get; set; }
    public List<AtributoServiceModel> Atributos { get; set; } = [];
    public List<TagServiceModel> Tags { get; set; } = [];
    public DateTime DataCriacao { get; set; }
    public DateTime DataAtualizacao { get; set; }
}

public class SalvarRegistroServiceModel
{
    public int IdTipoRegistro { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Resumo { get; set; }
    public string? Descricao { get; set; }
    public List<AtributoServiceModel> Atributos { get; set; } = [];
    public List<string> Tags { get; set; } = [];
}

public interface IRegistroService
{
    Task<IEnumerable<TipoRegistroServiceModel>> ListarTiposAsync();
    Task<IEnumerable<ModeloAtributoServiceModel>> ListarModelosAsync(int idSistema, int? idTipoRegistro);
    Task<IEnumerable<TagServiceModel>> ListarTagsAsync(int idUsuario, int idCampanha);
    Task<IEnumerable<RegistroResumoServiceModel>> ListarAsync(int idUsuario, int idCampanha, FiltroRegistro filtro);

    /// <summary>Busca rápida (Ctrl+K): até 10 registros. Termo vazio → lista vazia.</summary>
    Task<IEnumerable<RegistroResumoServiceModel>> BuscarAsync(int idUsuario, int idCampanha, string? termo);

    Task<RegistroServiceModel> ObterAsync(int idUsuario, int id);
    Task<RegistroServiceModel> InserirAsync(int idUsuario, int idCampanha, SalvarRegistroServiceModel dados);
    Task<RegistroServiceModel> AtualizarAsync(int idUsuario, int id, SalvarRegistroServiceModel dados);
    Task ExcluirAsync(int idUsuario, int id);
}

public class RegistroService : IRegistroService
{
    private const int OrigemCriado = 1;

    // Mesma regra do collation do banco (utf8mb4_0900_ai_ci): sem diferenciar maiúsculas nem acentos.
    private static readonly CompareInfo Comparador = CultureInfo.InvariantCulture.CompareInfo;
    private const CompareOptions SemMaiusculasNemAcentos = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;

    private readonly IRegistroRepository _registroRepository;
    private readonly IAtributoRegistroRepository _atributoRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IRegistroTagRepository _registroTagRepository;
    private readonly ITipoRegistroRepository _tipoRepository;
    private readonly IModeloAtributoRepository _modeloRepository;
    private readonly ICampanhaService _campanhaService;

    public RegistroService(
        IRegistroRepository registroRepository,
        IAtributoRegistroRepository atributoRepository,
        ITagRepository tagRepository,
        IRegistroTagRepository registroTagRepository,
        ITipoRegistroRepository tipoRepository,
        IModeloAtributoRepository modeloRepository,
        ICampanhaService campanhaService)
    {
        _registroRepository = registroRepository;
        _atributoRepository = atributoRepository;
        _tagRepository = tagRepository;
        _registroTagRepository = registroTagRepository;
        _tipoRepository = tipoRepository;
        _modeloRepository = modeloRepository;
        _campanhaService = campanhaService;
    }

    public static bool MesmoTexto(string a, string b) => Comparador.Compare(a, b, SemMaiusculasNemAcentos) == 0;

    /// <summary>Apara, remove vazias e repetidas (sem maiúsculas nem acentos), mantendo a primeira grafia.</summary>
    public static List<string> NormalizarTags(IEnumerable<string> tags)
    {
        var resultado = new List<string>();
        foreach (var tag in tags.Select(t => (t ?? string.Empty).Trim()).Where(t => t.Length > 0))
        {
            if (!resultado.Any(existente => MesmoTexto(existente, tag)))
                resultado.Add(tag);
        }
        return resultado;
    }

    /// <summary>Apara chave e valor, descarta os de valor vazio e numera na ordem recebida.</summary>
    public static List<AtributoRegistro> NormalizarAtributos(IEnumerable<AtributoServiceModel> atributos) =>
        atributos
            .Select(a => (Chave: (a.Chave ?? string.Empty).Trim(), Valor: (a.Valor ?? string.Empty).Trim()))
            .Where(a => a.Valor.Length > 0)
            .Select((a, indice) => new AtributoRegistro { Chave = a.Chave, Valor = a.Valor, Ordem = indice + 1 })
            .ToList();

    public async Task<IEnumerable<TipoRegistroServiceModel>> ListarTiposAsync() =>
        (await _tipoRepository.ListarAsync()).Select(t => new TipoRegistroServiceModel { Id = t.Id, Nome = t.Nome }).ToList();

    public async Task<IEnumerable<ModeloAtributoServiceModel>> ListarModelosAsync(int idSistema, int? idTipoRegistro) =>
        (await _modeloRepository.ListarAsync(idSistema, idTipoRegistro))
            .Select(m => new ModeloAtributoServiceModel { IdTipoRegistro = m.IdTipoRegistro, Chave = m.Chave, Ordem = m.Ordem })
            .ToList();

    public async Task<IEnumerable<TagServiceModel>> ListarTagsAsync(int idUsuario, int idCampanha)
    {
        await _campanhaService.ObterDoUsuarioOuLancarAsync(idUsuario, idCampanha);
        return (await _tagRepository.ListarPorCampanhaAsync(idCampanha)).Select(ParaServiceModel).ToList();
    }

    public async Task<IEnumerable<RegistroResumoServiceModel>> ListarAsync(int idUsuario, int idCampanha, FiltroRegistro filtro)
    {
        await _campanhaService.ObterDoUsuarioOuLancarAsync(idUsuario, idCampanha);

        var texto = filtro.Texto?.Trim();
        var filtroLimpo = new FiltroRegistro
        {
            IdTipoRegistro = filtro.IdTipoRegistro,
            IdTag = filtro.IdTag,
            Texto = string.IsNullOrEmpty(texto) ? null : texto,
        };

        return await ResumirComTagsAsync(await _registroRepository.ListarAsync(idCampanha, filtroLimpo));
    }

    public const int LimiteBusca = 10;

    public async Task<IEnumerable<RegistroResumoServiceModel>> BuscarAsync(int idUsuario, int idCampanha, string? termo)
    {
        await _campanhaService.ObterDoUsuarioOuLancarAsync(idUsuario, idCampanha);

        var termoLimpo = termo?.Trim();
        if (string.IsNullOrEmpty(termoLimpo))
            return [];

        return await ResumirComTagsAsync(await _registroRepository.BuscarAsync(idCampanha, termoLimpo, LimiteBusca));
    }

    private async Task<List<RegistroResumoServiceModel>> ResumirComTagsAsync(IEnumerable<Registro> encontrados)
    {
        var registros = encontrados.ToList();
        var tagsPorRegistro = (await _registroTagRepository.ListarPorRegistrosAsync(registros.Select(r => r.Id)))
            .ToLookup(x => x.IdRegistro, x => ParaServiceModel(x.Tag));

        return registros.Select(r => new RegistroResumoServiceModel
        {
            Id = r.Id,
            IdTipoRegistro = r.IdTipoRegistro,
            Nome = r.Nome,
            Resumo = r.Resumo,
            Tags = tagsPorRegistro[r.Id].ToList(),
        }).ToList();
    }

    public async Task<RegistroServiceModel> ObterAsync(int idUsuario, int id)
    {
        var registro = await ObterDoUsuarioOuLancarAsync(idUsuario, id);
        return await MontarAsync(registro);
    }

    public async Task<RegistroServiceModel> InserirAsync(int idUsuario, int idCampanha, SalvarRegistroServiceModel dados)
    {
        await _campanhaService.ObterDoUsuarioOuLancarAsync(idUsuario, idCampanha);
        await ValidarTipoAsync(dados.IdTipoRegistro);

        var agora = DateTime.Now;
        var registro = new Registro
        {
            IdCampanha = idCampanha,
            IdOrigemRegistro = OrigemCriado,
            DataCriacao = agora,
            DataAtualizacao = agora,
        };
        CopiarDados(dados, registro);

        using (var transacao = Transacao.Iniciar())
        {
            registro.Id = await _registroRepository.InserirAsync(registro);
            await GravarAtributosETagsAsync(registro, dados);
            transacao.Complete();
        }

        return await MontarAsync(registro);
    }

    public async Task<RegistroServiceModel> AtualizarAsync(int idUsuario, int id, SalvarRegistroServiceModel dados)
    {
        var registro = await ObterDoUsuarioOuLancarAsync(idUsuario, id);
        await ValidarTipoAsync(dados.IdTipoRegistro);

        CopiarDados(dados, registro);
        registro.DataAtualizacao = DateTime.Now;

        using (var transacao = Transacao.Iniciar())
        {
            await _registroRepository.AtualizarAsync(registro);
            await GravarAtributosETagsAsync(registro, dados);
            transacao.Complete();
        }

        return await MontarAsync(registro);
    }

    public async Task ExcluirAsync(int idUsuario, int id)
    {
        await ObterDoUsuarioOuLancarAsync(idUsuario, id);
        await _registroRepository.ExcluirAsync(id);
    }

    /// <summary>Registro inexistente ou de campanha de outro usuário → RegistroNaoEncontrado (404).</summary>
    private async Task<Registro> ObterDoUsuarioOuLancarAsync(int idUsuario, int id)
    {
        var registro = await _registroRepository.ObterAsync(id)
                       ?? throw new BusinessRuleException(BusinessErrors.RegistroNaoEncontrado);
        try
        {
            await _campanhaService.ObterDoUsuarioOuLancarAsync(idUsuario, registro.IdCampanha);
        }
        catch (BusinessRuleException e) when (e.Code == BusinessErrors.CampanhaNaoEncontrada.Code)
        {
            throw new BusinessRuleException(BusinessErrors.RegistroNaoEncontrado);
        }
        return registro;
    }

    private async Task ValidarTipoAsync(int idTipoRegistro)
    {
        var tipos = await _tipoRepository.ListarAsync();
        if (tipos.All(t => t.Id != idTipoRegistro))
            throw new BusinessRuleException(BusinessErrors.TipoRegistroInvalido);
    }

    private static void CopiarDados(SalvarRegistroServiceModel dados, Registro registro)
    {
        registro.IdTipoRegistro = dados.IdTipoRegistro;
        registro.Nome = dados.Nome.Trim();
        registro.Resumo = TextoOuNulo(dados.Resumo);
        registro.Descricao = TextoOuNulo(dados.Descricao);
    }

    /// <summary>Substitui os atributos e as tags do registro, criando as tags que ainda não existem.</summary>
    private async Task GravarAtributosETagsAsync(Registro registro, SalvarRegistroServiceModel dados)
    {
        await _atributoRepository.SubstituirAsync(registro.Id, NormalizarAtributos(dados.Atributos));

        var nomes = NormalizarTags(dados.Tags);
        var existentes = (await _tagRepository.ListarPorNomesAsync(registro.IdCampanha, nomes)).ToList();
        var idsTag = new List<int>();
        foreach (var nome in nomes)
        {
            var existente = existentes.FirstOrDefault(t => MesmoTexto(t.Nome, nome));
            idsTag.Add(existente?.Id
                       ?? await _tagRepository.InserirAsync(new Tag { IdCampanha = registro.IdCampanha, Nome = nome }));
        }
        await _registroTagRepository.SubstituirAsync(registro.Id, idsTag);
    }

    private async Task<RegistroServiceModel> MontarAsync(Registro registro)
    {
        var atributos = await _atributoRepository.ListarPorRegistroAsync(registro.Id);
        var tags = await _registroTagRepository.ListarPorRegistrosAsync([registro.Id]);

        return new RegistroServiceModel
        {
            Id = registro.Id,
            IdCampanha = registro.IdCampanha,
            IdTipoRegistro = registro.IdTipoRegistro,
            Nome = registro.Nome,
            Resumo = registro.Resumo,
            Descricao = registro.Descricao,
            Atributos = atributos.OrderBy(a => a.Ordem)
                .Select(a => new AtributoServiceModel { Chave = a.Chave, Valor = a.Valor }).ToList(),
            Tags = tags.Select(t => ParaServiceModel(t.Tag)).ToList(),
            DataCriacao = registro.DataCriacao,
            DataAtualizacao = registro.DataAtualizacao,
        };
    }

    private static TagServiceModel ParaServiceModel(Tag tag) => new() { Id = tag.Id, Nome = tag.Nome };

    private static string? TextoOuNulo(string? texto) =>
        string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
}
