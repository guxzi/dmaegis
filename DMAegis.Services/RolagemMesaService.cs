using DMAegis.Model;
using DMAegis.Repository;
using DMAegis.Services.Exceptions;

namespace DMAegis.Services;

public class RolagemServiceModel
{
    public int Id { get; set; }
    public string Autor { get; set; } = string.Empty;
    public bool Mestre { get; set; }
    public string Expressao { get; set; } = string.Empty;
    public int Total { get; set; }
    public string Detalhe { get; set; } = string.Empty;
    public bool Secreta { get; set; }
    public DateTime Data { get; set; }
}

public class RolarServiceModel
{
    public string Expressao { get; set; } = string.Empty;
    public int Total { get; set; }
    public string Detalhe { get; set; } = string.Empty;

    /// <summary>Só vale para o mestre: a rolagem fica escondida dos jogadores.</summary>
    public bool Secreta { get; set; }
}

public interface IRolagemMesaService
{
    Task<IEnumerable<RolagemServiceModel>> ListarDoMestreAsync(int idUsuario, int idMesa);
    Task<RolagemServiceModel> RolarDoMestreAsync(int idUsuario, int idMesa, RolarServiceModel dados);

    /// <summary>As rolagens da mesa ativa, sem as secretas. Sem mesa ativa, lista vazia.</summary>
    Task<IEnumerable<RolagemServiceModel>> ListarDoJogadorAsync(JogadorLogadoServiceModel jogador);
    Task<RolagemServiceModel> RolarDoJogadorAsync(JogadorLogadoServiceModel jogador, RolarServiceModel dados);

    /// <summary>O jogador rola a iniciativa: vira a iniciativa da ficha dele e aparece nas rolagens da mesa.</summary>
    Task<RolagemServiceModel> RolarIniciativaDoJogadorAsync(JogadorLogadoServiceModel jogador, RolarServiceModel dados);
}

/// <summary>
/// Rolagens da mesa. Os dados são rolados no navegador (regra do projeto); aqui só se guarda o resultado
/// e se avisa as telas abertas. Entre amigos, confiar no navegador do jogador é aceitável.
/// </summary>
public class RolagemMesaService : IRolagemMesaService
{
    public const int Limite = 30;
    public const string AutorMestre = "Mestre";

    private readonly IRolagemMesaRepository _rolagemRepository;
    private readonly IMesaRepository _mesaRepository;
    private readonly ICampanhaService _campanhaService;
    private readonly IJogadorService _jogadorService;
    private readonly ICanalMesa _canal;
    private readonly IParticipanteMesaRepository _participanteRepository;

    public RolagemMesaService(IRolagemMesaRepository rolagemRepository, IMesaRepository mesaRepository,
        ICampanhaService campanhaService, IJogadorService jogadorService, ICanalMesa canal,
        IParticipanteMesaRepository participanteRepository)
    {
        _participanteRepository = participanteRepository;
        _rolagemRepository = rolagemRepository;
        _mesaRepository = mesaRepository;
        _campanhaService = campanhaService;
        _jogadorService = jogadorService;
        _canal = canal;
    }

    public async Task<IEnumerable<RolagemServiceModel>> ListarDoMestreAsync(int idUsuario, int idMesa)
    {
        await ObterMesaDoUsuarioOuLancarAsync(idUsuario, idMesa);
        return (await _rolagemRepository.ListarRecentesAsync(idMesa, incluirSecretas: true, Limite)).Select(ParaServiceModel).ToList();
    }

    public async Task<RolagemServiceModel> RolarDoMestreAsync(int idUsuario, int idMesa, RolarServiceModel dados)
    {
        var mesa = await ObterMesaDoUsuarioOuLancarAsync(idUsuario, idMesa);
        return await GravarAsync(mesa, AutorMestre, mestre: true, dados, dados.Secreta);
    }

    public async Task<IEnumerable<RolagemServiceModel>> ListarDoJogadorAsync(JogadorLogadoServiceModel jogador)
    {
        await _jogadorService.ConferirAcessoAsync(jogador);
        var mesa = await _mesaRepository.ObterAtivaAsync(jogador.IdCampanha);
        if (mesa is null)
            return [];
        return (await _rolagemRepository.ListarRecentesAsync(mesa.Id, incluirSecretas: false, Limite)).Select(ParaServiceModel).ToList();
    }

    public async Task<RolagemServiceModel> RolarDoJogadorAsync(JogadorLogadoServiceModel jogador, RolarServiceModel dados)
    {
        await _jogadorService.ConferirAcessoAsync(jogador);
        var mesa = await _mesaRepository.ObterAtivaAsync(jogador.IdCampanha)
                   ?? throw new BusinessRuleException(BusinessErrors.MesaNaoEncontrada);
        // Jogador nunca rola em segredo: a rolagem dele aparece para todos.
        return await GravarAsync(mesa, jogador.NomePersonagem, mestre: false, dados, secreta: false);
    }

    public async Task<RolagemServiceModel> RolarIniciativaDoJogadorAsync(JogadorLogadoServiceModel jogador, RolarServiceModel dados)
    {
        await _jogadorService.ConferirAcessoAsync(jogador);
        var mesa = await _mesaRepository.ObterAtivaAsync(jogador.IdCampanha)
                   ?? throw new BusinessRuleException(BusinessErrors.MesaNaoEncontrada);
        var minha = (await _participanteRepository.ListarPorMesaAsync(mesa.Id))
                    .FirstOrDefault(p => p.IdRegistro == jogador.IdRegistro && p.Visivel)
                    ?? throw new BusinessRuleException(BusinessErrors.FichaForaDaMesa);
        // Fora do combate pode rolar de novo; com o combate andando, só quem ainda não tem iniciativa.
        if (mesa.EmCombate && minha.Iniciativa.HasValue)
            throw new BusinessRuleException(BusinessErrors.IniciativaJaDefinida);

        await _participanteRepository.AtualizarIniciativaAsync(minha.Id, dados.Total);
        var expressao = dados.Expressao.Trim();
        dados.Expressao = expressao.Length > 186 ? expressao[..186] : $"{expressao} (iniciativa)";
        // O painel de rolagens mostra o detalhe: ele também diz que é iniciativa.
        var detalhe = $"Iniciativa: {dados.Detalhe.Trim()}";
        dados.Detalhe = detalhe.Length > 500 ? detalhe[..500] : detalhe;
        return await GravarAsync(mesa, jogador.NomePersonagem, mestre: false, dados, secreta: false);
    }

    private async Task<RolagemServiceModel> GravarAsync(Mesa mesa, string autor, bool mestre, RolarServiceModel dados, bool secreta)
    {
        var agora = DateTime.Now;
        var rolagem = new RolagemMesa
        {
            IdMesa = mesa.Id,
            Autor = autor.Length > 100 ? autor[..100] : autor,
            Mestre = mestre,
            Expressao = dados.Expressao.Trim(),
            Total = dados.Total,
            Detalhe = dados.Detalhe.Trim(),
            Secreta = secreta,
            Data = new DateTime(agora.Year, agora.Month, agora.Day, agora.Hour, agora.Minute, agora.Second, DateTimeKind.Unspecified),
        };
        rolagem.Id = await _rolagemRepository.InserirAsync(rolagem);
        // Rolagem secreta não precisa avisar ninguém além do próprio mestre, que já tem a resposta.
        if (!secreta)
            _canal.Avisar(mesa.IdCampanha);
        return ParaServiceModel(rolagem);
    }

    private async Task<Mesa> ObterMesaDoUsuarioOuLancarAsync(int idUsuario, int idMesa)
    {
        var mesa = await _mesaRepository.ObterAsync(idMesa) ?? throw new BusinessRuleException(BusinessErrors.MesaNaoEncontrada);
        try
        {
            await _campanhaService.ObterDoUsuarioOuLancarAsync(idUsuario, mesa.IdCampanha);
        }
        catch (BusinessRuleException e) when (e.Code == BusinessErrors.CampanhaNaoEncontrada.Code)
        {
            throw new BusinessRuleException(BusinessErrors.MesaNaoEncontrada);
        }
        return mesa;
    }

    private static RolagemServiceModel ParaServiceModel(RolagemMesa r) => new()
    {
        Id = r.Id,
        Autor = r.Autor,
        Mestre = r.Mestre,
        Expressao = r.Expressao,
        Total = r.Total,
        Detalhe = r.Detalhe,
        Secreta = r.Secreta,
        Data = r.Data,
    };
}
