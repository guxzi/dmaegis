using DMAegis.Model;
using DMAegis.Repository;
using DMAegis.Services.Exceptions;

namespace DMAegis.Services;

public class SistemaServiceModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}

public class CampanhaServiceModel
{
    public int Id { get; set; }
    public int IdSistema { get; set; }
    public string NomeSistema { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public bool Arquivada { get; set; }
    public DateTime DataCriacao { get; set; }
}

public class InserirCampanhaServiceModel
{
    public int IdSistema { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
}

/// <summary>Só nome e descrição: o sistema não pode ser trocado depois de criada a campanha.</summary>
public class AtualizarCampanhaServiceModel
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
}

public interface ICampanhaService
{
    Task<IEnumerable<SistemaServiceModel>> ListarSistemasAsync();
    Task<IEnumerable<CampanhaServiceModel>> ListarAsync(int idUsuario);
    Task<CampanhaServiceModel> ObterAsync(int idUsuario, int id);
    Task<CampanhaServiceModel> InserirAsync(int idUsuario, InserirCampanhaServiceModel dados);
    Task<CampanhaServiceModel> AtualizarAsync(int idUsuario, int id, AtualizarCampanhaServiceModel dados);
    Task<CampanhaServiceModel> DefinirArquivadaAsync(int idUsuario, int id, bool arquivada);
    Task ExcluirAsync(int idUsuario, int id);

    /// <summary>
    /// Devolve a campanha se ela pertence ao usuário. Se não existe OU é de outro usuário,
    /// lança CampanhaNaoEncontrada (404) — assim não se revela que o id existe.
    /// Toda operação em algo da campanha (registros, sessões…) passa por aqui.
    /// </summary>
    Task<Campanha> ObterDoUsuarioOuLancarAsync(int idUsuario, int idCampanha);
}

public class CampanhaService : ICampanhaService
{
    private readonly ICampanhaRepository _campanhaRepository;
    private readonly ISistemaRepository _sistemaRepository;

    public CampanhaService(ICampanhaRepository campanhaRepository, ISistemaRepository sistemaRepository)
    {
        _campanhaRepository = campanhaRepository;
        _sistemaRepository = sistemaRepository;
    }

    public async Task<IEnumerable<SistemaServiceModel>> ListarSistemasAsync() =>
        (await _sistemaRepository.ListarAsync()).Select(s => new SistemaServiceModel { Id = s.Id, Nome = s.Nome });

    public async Task<IEnumerable<CampanhaServiceModel>> ListarAsync(int idUsuario)
    {
        var sistemas = await NomesDosSistemasAsync();
        var campanhas = await _campanhaRepository.ListarPorUsuarioAsync(idUsuario);
        return campanhas.Select(c => ParaServiceModel(c, sistemas)).ToList();
    }

    public async Task<CampanhaServiceModel> ObterAsync(int idUsuario, int id)
    {
        var campanha = await ObterDoUsuarioOuLancarAsync(idUsuario, id);
        return ParaServiceModel(campanha, await NomesDosSistemasAsync());
    }

    public async Task<CampanhaServiceModel> InserirAsync(int idUsuario, InserirCampanhaServiceModel dados)
    {
        var sistemas = await NomesDosSistemasAsync();
        if (!sistemas.ContainsKey(dados.IdSistema))
            throw new BusinessRuleException(BusinessErrors.SistemaInvalido);

        var campanha = new Campanha
        {
            IdUsuario = idUsuario,
            IdSistema = dados.IdSistema,
            Nome = dados.Nome.Trim(),
            Descricao = TextoOuNulo(dados.Descricao),
            Arquivada = false,
            DataCriacao = DateTime.Now,
        };
        campanha.Id = await _campanhaRepository.InserirAsync(campanha);

        return ParaServiceModel(campanha, sistemas);
    }

    public async Task<CampanhaServiceModel> AtualizarAsync(int idUsuario, int id, AtualizarCampanhaServiceModel dados)
    {
        var campanha = await ObterDoUsuarioOuLancarAsync(idUsuario, id);
        campanha.Nome = dados.Nome.Trim();
        campanha.Descricao = TextoOuNulo(dados.Descricao);
        await _campanhaRepository.AtualizarAsync(campanha);

        return ParaServiceModel(campanha, await NomesDosSistemasAsync());
    }

    public async Task<CampanhaServiceModel> DefinirArquivadaAsync(int idUsuario, int id, bool arquivada)
    {
        var campanha = await ObterDoUsuarioOuLancarAsync(idUsuario, id);
        campanha.Arquivada = arquivada;
        await _campanhaRepository.AtualizarAsync(campanha);

        return ParaServiceModel(campanha, await NomesDosSistemasAsync());
    }

    public async Task ExcluirAsync(int idUsuario, int id)
    {
        await ObterDoUsuarioOuLancarAsync(idUsuario, id);
        await _campanhaRepository.ExcluirAsync(id);
    }

    public async Task<Campanha> ObterDoUsuarioOuLancarAsync(int idUsuario, int idCampanha)
    {
        var campanha = await _campanhaRepository.ObterAsync(idCampanha);
        if (campanha is null || campanha.IdUsuario != idUsuario)
            throw new BusinessRuleException(BusinessErrors.CampanhaNaoEncontrada);
        return campanha;
    }

    private async Task<Dictionary<int, string>> NomesDosSistemasAsync() =>
        (await _sistemaRepository.ListarAsync()).ToDictionary(s => s.Id, s => s.Nome);

    private static string? TextoOuNulo(string? texto) =>
        string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();

    private static CampanhaServiceModel ParaServiceModel(Campanha campanha, Dictionary<int, string> sistemas) => new()
    {
        Id = campanha.Id,
        IdSistema = campanha.IdSistema,
        NomeSistema = sistemas.GetValueOrDefault(campanha.IdSistema, string.Empty),
        Nome = campanha.Nome,
        Descricao = campanha.Descricao,
        Arquivada = campanha.Arquivada,
        DataCriacao = campanha.DataCriacao,
    };
}
