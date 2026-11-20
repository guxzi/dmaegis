using DMAegis.Model;
using DMAegis.Repository;
using DMAegis.Services.Exceptions;

namespace DMAegis.Services;

public class MesaResumoServiceModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int? IdImagemRegistro { get; set; }
    public bool Ativa { get; set; }
    public bool EmCombate { get; set; }
}

public class ParticipanteServiceModel
{
    public int Id { get; set; }
    public int? IdRegistro { get; set; }
    public int? IdTipoRegistro { get; set; }
    public int? IdImagemRetrato { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool Jogador { get; set; }
    public int? Iniciativa { get; set; }
    public int BonusIniciativa { get; set; }
    public int? Ca { get; set; }
    public int? PvAtual { get; set; }
    public int? PvMaximo { get; set; }
    public int? Coluna { get; set; }
    public int? Linha { get; set; }
    public int Tamanho { get; set; }
    public bool Visivel { get; set; }
    public int Ordem { get; set; }
    public List<string> Condicoes { get; set; } = [];
}

public class CelulaServiceModel
{
    public int Coluna { get; set; }
    public int Linha { get; set; }
}

public class MesaServiceModel
{
    public int Id { get; set; }
    public int IdCampanha { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int? IdImagemRegistro { get; set; }
    public int TamanhoCelula { get; set; }
    public int DeslocamentoX { get; set; }
    public int DeslocamentoY { get; set; }
    public bool GradeVisivel { get; set; }
    public bool MovimentoLivre { get; set; }
    public bool Nevoa { get; set; }
    public string? Clima { get; set; }
    public List<CelulaServiceModel> CelulasReveladas { get; set; } = [];
    public bool Ativa { get; set; }
    public bool EmCombate { get; set; }
    public int Rodada { get; set; }
    public int? IdParticipanteVez { get; set; }

    /// <summary>Na ordem da iniciativa (quem não tem iniciativa vai para o fim, na ordem de criação).</summary>
    public List<ParticipanteServiceModel> Participantes { get; set; } = [];
}

public class SalvarMesaServiceModel
{
    public string Nome { get; set; } = string.Empty;
    public int? IdImagemRegistro { get; set; }
    public int TamanhoCelula { get; set; } = 70;
    public int DeslocamentoX { get; set; }
    public int DeslocamentoY { get; set; }
    public bool GradeVisivel { get; set; } = true;
    public bool MovimentoLivre { get; set; } = true;
    public bool Nevoa { get; set; }
    public string? Clima { get; set; }
}

public class InserirParticipantesServiceModel
{
    /// <summary>Verdadeiro = todos os PJs da campanha que ainda não estão na mesa (ignora os outros campos).</summary>
    public bool TodosJogadores { get; set; }
    public int? IdRegistro { get; set; }

    /// <summary>Obrigatório para ficha avulsa; com registro, substitui o nome dele.</summary>
    public string? Nome { get; set; }
    public int Quantidade { get; set; } = 1;
}

public class SalvarParticipanteServiceModel
{
    public string Nome { get; set; } = string.Empty;
    public int? Iniciativa { get; set; }
    public int BonusIniciativa { get; set; }
    public int? Ca { get; set; }
    public int? PvAtual { get; set; }
    public int? PvMaximo { get; set; }
    public int Tamanho { get; set; } = 1;
    public bool Visivel { get; set; } = true;
    public int Ordem { get; set; }
    public List<string> Condicoes { get; set; } = [];
}

public class IniciativaServiceModel
{
    public int IdParticipante { get; set; }
    public int? Iniciativa { get; set; }
}

/// <summary>Ficha como o jogador a vê: sem PV nem CA de quem não é PJ.</summary>
public class ParticipanteVisaoServiceModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool Jogador { get; set; }
    public int? IdTipoRegistro { get; set; }
    public int? IdImagemRetrato { get; set; }
    public int? Coluna { get; set; }
    public int? Linha { get; set; }
    public int Tamanho { get; set; }
    public int? Ca { get; set; }
    public int? PvAtual { get; set; }
    public int? PvMaximo { get; set; }
    public string? EstadoSaude { get; set; }
    public List<string> Condicoes { get; set; } = [];
    public bool EhMinha { get; set; }

    /// <summary>Só dos PJs (o jogador rola a própria iniciativa com o bônus).</summary>
    public int? Iniciativa { get; set; }
    public int? BonusIniciativa { get; set; }
}

public class VisaoJogadorServiceModel
{
    public int IdMesa { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int? IdImagemRegistro { get; set; }
    public int TamanhoCelula { get; set; }
    public int DeslocamentoX { get; set; }
    public int DeslocamentoY { get; set; }
    public bool GradeVisivel { get; set; }
    public bool MovimentoLivre { get; set; }
    public bool Nevoa { get; set; }
    public string? Clima { get; set; }
    public List<CelulaServiceModel> CelulasReveladas { get; set; } = [];
    public bool EmCombate { get; set; }
    public int Rodada { get; set; }
    public int? IdParticipanteVez { get; set; }
    public List<ParticipanteVisaoServiceModel> Participantes { get; set; } = [];
}

public interface IMesaService
{
    Task<IEnumerable<MesaResumoServiceModel>> ListarAsync(int idUsuario, int idCampanha);
    Task<MesaServiceModel> ObterAsync(int idUsuario, int id);
    Task<MesaServiceModel> InserirAsync(int idUsuario, int idCampanha, SalvarMesaServiceModel dados);
    Task<MesaServiceModel> AtualizarAsync(int idUsuario, int id, SalvarMesaServiceModel dados);
    Task ExcluirAsync(int idUsuario, int id);
    Task<MesaServiceModel> AtivarAsync(int idUsuario, int id);

    Task<MesaServiceModel> InserirParticipantesAsync(int idUsuario, int idMesa, InserirParticipantesServiceModel dados);
    Task<MesaServiceModel> AtualizarParticipanteAsync(int idUsuario, int id, SalvarParticipanteServiceModel dados);
    Task MoverParticipanteAsync(int idUsuario, int id, int? coluna, int? linha);
    Task<MesaServiceModel> ExcluirParticipanteAsync(int idUsuario, int id);

    Task<MesaServiceModel> IniciarCombateAsync(int idUsuario, int idMesa, IEnumerable<IniciativaServiceModel> iniciativas);
    Task<MesaServiceModel> ProximoTurnoAsync(int idUsuario, int idMesa);
    Task<MesaServiceModel> EncerrarCombateAsync(int idUsuario, int idMesa);

    Task<VisaoJogadorServiceModel> ObterVisaoJogadorAsync(int idUsuario, int idMesa);

    /// <summary>Revela e esconde células da névoa num lote só (o que o pincel pintou de uma vez).</summary>
    Task<MesaServiceModel> AlterarNevoaAsync(int idUsuario, int idMesa,
        IEnumerable<CelulaServiceModel> revelar, IEnumerable<CelulaServiceModel> esconder);

    /// <summary>Cobre a mesa inteira de novo.</summary>
    Task<MesaServiceModel> EsconderTudoAsync(int idUsuario, int idMesa);

    /// <summary>Sinal passageiro do mestre (ping, reação ou arrasto ao vivo). Nada é gravado.</summary>
    Task SinalizarAsync(int idUsuario, int idMesa, SinalMesaServiceModel sinal);

    /// <summary>
    /// Estado completo da mesa ativa da campanha, sem conferir dono: só para o JogadorService,
    /// que já conferiu o convite e sempre filtra com FiltrarParaJogador antes de responder.
    /// </summary>
    Task<MesaServiceModel?> MontarAtivaAsync(int idCampanha);
}

public class MesaService : IMesaService
{
    public const int MaximoParticipantes = 60;
    public const int IdTipoJogador = 2;

    private readonly IMesaRepository _mesaRepository;
    private readonly IParticipanteMesaRepository _participanteRepository;
    private readonly ICondicaoParticipanteRepository _condicaoRepository;
    private readonly IRegistroRepository _registroRepository;
    private readonly IAtributoRegistroRepository _atributoRepository;
    private readonly IImagemRegistroRepository _imagemRepository;
    private readonly ICampanhaService _campanhaService;
    private readonly ICanalMesa _canal;
    private readonly IEventoMesaRepository _eventoRepository;
    private readonly ICelulaReveladaRepository _celulaRepository;

    public MesaService(
        IMesaRepository mesaRepository,
        IParticipanteMesaRepository participanteRepository,
        ICondicaoParticipanteRepository condicaoRepository,
        IRegistroRepository registroRepository,
        IAtributoRegistroRepository atributoRepository,
        IImagemRegistroRepository imagemRepository,
        ICampanhaService campanhaService,
        ICanalMesa canal,
        ICelulaReveladaRepository celulaRepository,
        IEventoMesaRepository eventoRepository)
    {
        _eventoRepository = eventoRepository;
        _mesaRepository = mesaRepository;
        _participanteRepository = participanteRepository;
        _condicaoRepository = condicaoRepository;
        _registroRepository = registroRepository;
        _atributoRepository = atributoRepository;
        _imagemRepository = imagemRepository;
        _campanhaService = campanhaService;
        _canal = canal;
        _celulaRepository = celulaRepository;
    }

    // ---------- Mesas ----------

    public async Task<IEnumerable<MesaResumoServiceModel>> ListarAsync(int idUsuario, int idCampanha)
    {
        await _campanhaService.ObterDoUsuarioOuLancarAsync(idUsuario, idCampanha);
        return (await _mesaRepository.ListarPorCampanhaAsync(idCampanha))
            .Select(m => new MesaResumoServiceModel
            {
                Id = m.Id, Nome = m.Nome, IdImagemRegistro = m.IdImagemRegistro, Ativa = m.Ativa, EmCombate = m.EmCombate,
            })
            .ToList();
    }

    public async Task<MesaServiceModel> ObterAsync(int idUsuario, int id) =>
        await MontarAsync(await ObterMesaDoUsuarioOuLancarAsync(idUsuario, id));

    public async Task<MesaServiceModel> InserirAsync(int idUsuario, int idCampanha, SalvarMesaServiceModel dados)
    {
        await _campanhaService.ObterDoUsuarioOuLancarAsync(idUsuario, idCampanha);
        await ConferirImagemDaCampanhaAsync(idCampanha, dados.IdImagemRegistro);

        var agora = Agora();
        // A primeira mesa da campanha já nasce ativa.
        var primeira = !(await _mesaRepository.ListarPorCampanhaAsync(idCampanha)).Any();
        var mesa = new Mesa
        {
            IdCampanha = idCampanha,
            Nome = dados.Nome.Trim(),
            IdImagemRegistro = dados.IdImagemRegistro,
            TamanhoCelula = dados.TamanhoCelula,
            DeslocamentoX = dados.DeslocamentoX,
            DeslocamentoY = dados.DeslocamentoY,
            GradeVisivel = dados.GradeVisivel,
            MovimentoLivre = dados.MovimentoLivre,
            Nevoa = dados.Nevoa,
            Clima = dados.Clima,
            Ativa = primeira,
            DataCriacao = agora,
            DataAtualizacao = agora,
        };
        mesa.Id = await _mesaRepository.InserirAsync(mesa);
        return await AvisarEMontarAsync(mesa);
    }

    public async Task<MesaServiceModel> AtualizarAsync(int idUsuario, int id, SalvarMesaServiceModel dados)
    {
        var mesa = await ObterMesaDoUsuarioOuLancarAsync(idUsuario, id);
        await ConferirImagemDaCampanhaAsync(mesa.IdCampanha, dados.IdImagemRegistro);

        mesa.Nome = dados.Nome.Trim();
        mesa.IdImagemRegistro = dados.IdImagemRegistro;
        mesa.TamanhoCelula = dados.TamanhoCelula;
        mesa.DeslocamentoX = dados.DeslocamentoX;
        mesa.DeslocamentoY = dados.DeslocamentoY;
        mesa.GradeVisivel = dados.GradeVisivel;
        mesa.MovimentoLivre = dados.MovimentoLivre;
        mesa.Nevoa = dados.Nevoa;
        mesa.Clima = dados.Clima;
        mesa.DataAtualizacao = Agora();
        await _mesaRepository.AtualizarAsync(mesa);
        return await AvisarEMontarAsync(mesa);
    }

    public async Task ExcluirAsync(int idUsuario, int id)
    {
        var mesa = await ObterMesaDoUsuarioOuLancarAsync(idUsuario, id);
        await _mesaRepository.ExcluirAsync(id);
        _canal.Avisar(mesa.IdCampanha);
    }

    public async Task<MesaServiceModel> AtivarAsync(int idUsuario, int id)
    {
        var mesa = await ObterMesaDoUsuarioOuLancarAsync(idUsuario, id);
        await _mesaRepository.DefinirAtivaAsync(mesa.IdCampanha, mesa.Id);
        mesa.Ativa = true;
        return await AvisarEMontarAsync(mesa);
    }

    // ---------- Participantes ----------

    public async Task<MesaServiceModel> InserirParticipantesAsync(int idUsuario, int idMesa, InserirParticipantesServiceModel dados)
    {
        var mesa = await ObterMesaDoUsuarioOuLancarAsync(idUsuario, idMesa);
        var existentes = (await _participanteRepository.ListarPorMesaAsync(idMesa)).ToList();
        var novos = dados.TodosJogadores
            ? await NovosJogadoresAsync(mesa, existentes)
            : await NovosDoPedidoAsync(mesa, dados, existentes);

        if (existentes.Count + novos.Count > MaximoParticipantes)
            throw new BusinessRuleException(BusinessErrors.LimiteParticipantes);

        var ordem = existentes.Count == 0 ? 0 : existentes.Max(p => p.Ordem) + 1;
        using (var transacao = Transacao.Iniciar())
        {
            foreach (var novo in novos)
            {
                novo.IdMesa = idMesa;
                novo.Ordem = ordem++;
                await _participanteRepository.InserirAsync(novo);
            }
            transacao.Complete();
        }
        return await AvisarEMontarAsync(mesa);
    }

    public async Task<MesaServiceModel> AtualizarParticipanteAsync(int idUsuario, int id, SalvarParticipanteServiceModel dados)
    {
        var (participante, mesa) = await ObterParticipanteDoUsuarioOuLancarAsync(idUsuario, id);

        var pvAntes = participante.PvAtual;
        participante.Nome = dados.Nome.Trim();
        participante.Iniciativa = dados.Iniciativa;
        participante.BonusIniciativa = dados.BonusIniciativa;
        participante.Ca = dados.Ca;
        participante.PvMaximo = dados.PvMaximo;
        // PV atual fica entre 0 e o máximo (quando há máximo).
        participante.PvAtual = dados.PvAtual is null ? null
            : Math.Max(0, dados.PvMaximo is null ? dados.PvAtual.Value : Math.Min(dados.PvAtual.Value, dados.PvMaximo.Value));
        participante.Tamanho = dados.Tamanho;
        participante.Visivel = dados.Visivel;
        participante.Ordem = dados.Ordem;

        var condicoes = dados.Condicoes
            .Select(c => c.Trim())
            .Where(c => c.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        using (var transacao = Transacao.Iniciar())
        {
            await _participanteRepository.AtualizarAsync(participante);
            await _condicaoRepository.SubstituirAsync(participante.Id, condicoes);
            transacao.Complete();
        }
        // Caiu agora (o PV chegou a zero): vai para o diário da mesa.
        if (participante.PvMaximo.HasValue && pvAntes is > 0 && participante.PvAtual is <= 0)
            await RegistrarAsync(mesa.Id, RegrasMesa.EventoCaiu, $"{participante.Nome} caiu.");
        return await AvisarEMontarAsync(mesa);
    }

    public async Task MoverParticipanteAsync(int idUsuario, int id, int? coluna, int? linha)
    {
        var (_, mesa) = await ObterParticipanteDoUsuarioOuLancarAsync(idUsuario, id);
        // Só fora do mapa quando os dois são nulos; meio a meio não existe.
        if (coluna is null || linha is null)
            (coluna, linha) = (null, null);
        await _participanteRepository.AtualizarPosicaoAsync(id, coluna, linha);
        _canal.Avisar(mesa.IdCampanha);
    }

    public async Task<MesaServiceModel> ExcluirParticipanteAsync(int idUsuario, int id)
    {
        var (_, mesa) = await ObterParticipanteDoUsuarioOuLancarAsync(idUsuario, id);
        await _participanteRepository.ExcluirAsync(id);
        _canal.Avisar(mesa.IdCampanha);
        // Se era a vez dele, o banco limpa a vez (SET NULL); a mesa é lida de novo para refletir isso.
        return await ObterAsync(idUsuario, mesa.Id);
    }

    // ---------- Combate ----------

    public async Task<MesaServiceModel> IniciarCombateAsync(int idUsuario, int idMesa, IEnumerable<IniciativaServiceModel> iniciativas)
    {
        var mesa = await ObterMesaDoUsuarioOuLancarAsync(idUsuario, idMesa);
        var participantes = (await _participanteRepository.ListarPorMesaAsync(idMesa)).ToList();
        var porId = participantes.ToDictionary(p => p.Id);

        using (var transacao = Transacao.Iniciar())
        {
            // Ids de outras mesas são ignorados.
            foreach (var item in iniciativas.Where(i => porId.ContainsKey(i.IdParticipante)))
            {
                porId[item.IdParticipante].Iniciativa = item.Iniciativa;
                await _participanteRepository.AtualizarIniciativaAsync(item.IdParticipante, item.Iniciativa);
            }

            var ordem = RegrasMesa.OrdenarIniciativa(participantes);
            var vez = RegrasMesa.PrimeiraVez(ordem);
            await _mesaRepository.AtualizarCombateAsync(idMesa, true, 1, vez);
            var nomes = string.Join(", ", ordem.Where(p => p.Visivel).Select(p => p.Iniciativa.HasValue ? $"{p.Nome} ({p.Iniciativa})" : p.Nome));
            await RegistrarAsync(idMesa, RegrasMesa.EventoCombateInicio, $"O combate começou. Ordem: {nomes}.");
            (mesa.EmCombate, mesa.Rodada, mesa.IdParticipanteVez) = (true, 1, vez);
            transacao.Complete();
        }
        return await AvisarEMontarAsync(mesa);
    }

    public async Task<MesaServiceModel> ProximoTurnoAsync(int idUsuario, int idMesa)
    {
        var mesa = await ObterMesaDoUsuarioOuLancarAsync(idUsuario, idMesa);
        if (!mesa.EmCombate)
            return await AvisarEMontarAsync(mesa);

        var ordem = RegrasMesa.OrdenarIniciativa(await _participanteRepository.ListarPorMesaAsync(idMesa));
        var (vez, rodada) = RegrasMesa.ProximaVez(ordem, mesa.IdParticipanteVez, Math.Max(mesa.Rodada, 1));
        await _mesaRepository.AtualizarCombateAsync(idMesa, true, rodada, vez);
        (mesa.Rodada, mesa.IdParticipanteVez) = (rodada, vez);
        return await AvisarEMontarAsync(mesa);
    }

    public async Task<MesaServiceModel> EncerrarCombateAsync(int idUsuario, int idMesa)
    {
        var mesa = await ObterMesaDoUsuarioOuLancarAsync(idUsuario, idMesa);
        if (mesa.EmCombate)
            await RegistrarAsync(idMesa, RegrasMesa.EventoCombateFim,
                mesa.Rodada == 1 ? "O combate terminou na primeira rodada." : $"O combate terminou depois de {mesa.Rodada} rodadas.");
        await _mesaRepository.AtualizarCombateAsync(idMesa, false, 0, null);
        (mesa.EmCombate, mesa.Rodada, mesa.IdParticipanteVez) = (false, 0, null);
        return await AvisarEMontarAsync(mesa);
    }

    // ---------- Visão dos jogadores ----------

    public async Task<VisaoJogadorServiceModel> ObterVisaoJogadorAsync(int idUsuario, int idMesa) =>
        FiltrarParaJogador(await ObterAsync(idUsuario, idMesa), idRegistroJogador: null);

    /// <summary>
    /// O único caminho do estado do mestre para o dos jogadores: tira as fichas ocultas e troca
    /// PV e CA de quem não é PJ pela saúde em palavras. Nada do caderno passa daqui.
    /// </summary>
    public static VisaoJogadorServiceModel FiltrarParaJogador(MesaServiceModel mesa, int? idRegistroJogador)
    {
        // Com névoa, quem não é PJ só aparece se todas as células que a ficha ocupa estiverem reveladas.
        var reveladas = mesa.CelulasReveladas.Select(c => (c.Coluna, c.Linha)).ToHashSet();
        var visiveis = mesa.Participantes
            .Where(p => p.Visivel && (!mesa.Nevoa || p.Jogador || ForaDaNevoa(p, reveladas)))
            .ToList();
        return new VisaoJogadorServiceModel
        {
            IdMesa = mesa.Id,
            Nome = mesa.Nome,
            IdImagemRegistro = mesa.IdImagemRegistro,
            TamanhoCelula = mesa.TamanhoCelula,
            DeslocamentoX = mesa.DeslocamentoX,
            DeslocamentoY = mesa.DeslocamentoY,
            GradeVisivel = mesa.GradeVisivel,
            MovimentoLivre = mesa.MovimentoLivre,
            Nevoa = mesa.Nevoa,
            Clima = mesa.Clima,
            CelulasReveladas = mesa.Nevoa ? [.. mesa.CelulasReveladas] : [],
            EmCombate = mesa.EmCombate,
            Rodada = mesa.Rodada,
            IdParticipanteVez = visiveis.Any(p => p.Id == mesa.IdParticipanteVez) ? mesa.IdParticipanteVez : null,
            Participantes = visiveis.Select(p => new ParticipanteVisaoServiceModel
            {
                Id = p.Id,
                Nome = p.Nome,
                Jogador = p.Jogador,
                IdTipoRegistro = p.IdTipoRegistro,
                IdImagemRetrato = p.IdImagemRetrato,
                Coluna = p.Coluna,
                Linha = p.Linha,
                Tamanho = p.Tamanho,
                Ca = p.Jogador ? p.Ca : null,
                PvAtual = p.Jogador ? p.PvAtual : null,
                PvMaximo = p.Jogador ? p.PvMaximo : null,
                EstadoSaude = RegrasMesa.EstadoSaude(p.PvAtual, p.PvMaximo),
                Condicoes = [.. p.Condicoes],
                EhMinha = idRegistroJogador.HasValue && p.IdRegistro == idRegistroJogador,
                Iniciativa = p.Jogador ? p.Iniciativa : null,
                BonusIniciativa = p.Jogador ? p.BonusIniciativa : null,
            }).ToList(),
        };
    }

    /// <summary>Ficha fora do mapa não aparece no palco; no mapa, precisa de todas as células reveladas.</summary>
    public static bool ForaDaNevoa(ParticipanteServiceModel p, HashSet<(int Coluna, int Linha)> reveladas)
    {
        if (p.Coluna is not int coluna || p.Linha is not int linha)
            return true;
        var lado = Math.Max(1, p.Tamanho);
        for (var dc = 0; dc < lado; dc++)
            for (var dl = 0; dl < lado; dl++)
                if (!reveladas.Contains((coluna + dc, linha + dl)))
                    return false;
        return true;
    }

    // ---------- Névoa ----------

    public const int MaximoCelulasPorLote = 5000;

    public async Task<MesaServiceModel> AlterarNevoaAsync(int idUsuario, int idMesa,
        IEnumerable<CelulaServiceModel> revelar, IEnumerable<CelulaServiceModel> esconder)
    {
        var mesa = await ObterMesaDoUsuarioOuLancarAsync(idUsuario, idMesa);
        var paraRevelar = revelar.Select(c => (c.Coluna, c.Linha)).Distinct().ToList();
        var paraEsconder = esconder.Select(c => (c.Coluna, c.Linha)).Distinct().Except(paraRevelar).ToList();
        if (paraRevelar.Count + paraEsconder.Count > MaximoCelulasPorLote)
            throw new BusinessRuleException((BusinessErrors.DadosInvalidos.Code, $"No máximo {MaximoCelulasPorLote} células por vez."));

        using (var transacao = Transacao.Iniciar())
        {
            await _celulaRepository.RevelarAsync(idMesa, paraRevelar);
            await _celulaRepository.EsconderAsync(idMesa, paraEsconder);
            transacao.Complete();
        }
        return await AvisarEMontarAsync(mesa);
    }

    public async Task<MesaServiceModel> EsconderTudoAsync(int idUsuario, int idMesa)
    {
        var mesa = await ObterMesaDoUsuarioOuLancarAsync(idUsuario, idMesa);
        await _celulaRepository.EsconderTudoAsync(idMesa);
        return await AvisarEMontarAsync(mesa);
    }

    // ---------- Apoio ----------

    // ---------- Sinais passageiros ----------

    public async Task SinalizarAsync(int idUsuario, int idMesa, SinalMesaServiceModel sinal)
    {
        var mesa = await ObterMesaDoUsuarioOuLancarAsync(idUsuario, idMesa);
        sinal.Autor = "Mestre";
        sinal.IdMesa = mesa.Id;
        // Os jogadores só veem a mesa ativa: sinal de outra mesa fica com o mestre.
        sinal.Publico = mesa.Ativa;
        if (sinal.Tipo is RegrasMesa.SinalArrasto or RegrasMesa.SinalArrastoFim)
        {
            var participante = sinal.IdParticipante is int id ? await _participanteRepository.ObterAsync(id) : null;
            if (participante is null || participante.IdMesa != mesa.Id)
                throw new BusinessRuleException(BusinessErrors.ParticipanteNaoEncontrado);
            // O arrasto ao vivo não pode entregar o que está escondido: ficha oculta nunca vai,
            // e com névoa só a ficha de PJ (um monstro sob a névoa apareceria pelo movimento).
            sinal.Publico = mesa.Ativa && participante.Visivel && (!mesa.Nevoa || participante.Jogador);
        }
        else
        {
            sinal.IdParticipante = null;
        }
        _canal.Sinalizar(mesa.IdCampanha, sinal);
    }

    private async Task<List<ParticipanteMesa>> NovosJogadoresAsync(Mesa mesa, List<ParticipanteMesa> existentes)
    {
        var naMesa = existentes.Where(p => p.IdRegistro.HasValue).Select(p => p.IdRegistro!.Value).ToHashSet();
        var jogadores = await _registroRepository.ListarAsync(mesa.IdCampanha, new FiltroRegistro { IdTipoRegistro = IdTipoJogador });
        var novos = new List<ParticipanteMesa>();
        foreach (var registro in jogadores.Where(r => !naMesa.Contains(r.Id)).OrderBy(r => r.Nome))
            novos.Add(await ParticipanteDoRegistroAsync(registro, registro.Nome));
        return novos;
    }

    private async Task<List<ParticipanteMesa>> NovosDoPedidoAsync(Mesa mesa, InserirParticipantesServiceModel dados, List<ParticipanteMesa> existentes)
    {
        var quantidade = Math.Clamp(dados.Quantidade, 1, MaximoParticipantes);
        if (dados.IdRegistro is null)
        {
            var nome = (dados.Nome ?? string.Empty).Trim();
            if (nome.Length == 0)
                throw new BusinessRuleException((BusinessErrors.DadosInvalidos.Code, "Informe o nome da ficha."));
            return RegrasMesa.NomesNumerados(nome, quantidade, existentes.Select(p => p.Nome))
                .Select(n => new ParticipanteMesa { Nome = n, Tamanho = 1, Visivel = true })
                .ToList();
        }

        var registro = await _registroRepository.ObterAsync(dados.IdRegistro.Value);
        if (registro is null || registro.IdCampanha != mesa.IdCampanha)
            throw new BusinessRuleException(BusinessErrors.RegistroNaoEncontrado);

        var nomeBase = string.IsNullOrWhiteSpace(dados.Nome) ? registro.Nome : dados.Nome.Trim();
        var novos = new List<ParticipanteMesa>();
        foreach (var nome in RegrasMesa.NomesNumerados(nomeBase, quantidade, existentes.Select(p => p.Nome)))
            novos.Add(await ParticipanteDoRegistroAsync(registro, nome));
        return novos;
    }

    /// <summary>Ficha a partir do caderno: CA, PV e bônus de iniciativa vêm dos atributos, quando existem.</summary>
    private async Task<ParticipanteMesa> ParticipanteDoRegistroAsync(Registro registro, string nome)
    {
        var atributos = (await _atributoRepository.ListarPorRegistroAsync(registro.Id))
            .GroupBy(a => a.Chave.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Valor, StringComparer.OrdinalIgnoreCase);
        int? Numero(string chave) => atributos.TryGetValue(chave, out var valor) ? RegrasMesa.NumeroDoAtributo(valor) : null;

        var pv = Numero("PV");
        return new ParticipanteMesa
        {
            IdRegistro = registro.Id,
            Nome = nome.Length > 100 ? nome[..100] : nome,
            Jogador = registro.IdTipoRegistro == IdTipoJogador,
            Ca = Numero("CA"),
            PvMaximo = pv,
            PvAtual = pv,
            BonusIniciativa = Numero("Iniciativa") ?? 0,
            Tamanho = 1,
            Visivel = true,
        };
    }

    public async Task<MesaServiceModel?> MontarAtivaAsync(int idCampanha)
    {
        var mesa = await _mesaRepository.ObterAtivaAsync(idCampanha);
        return mesa is null ? null : await MontarAsync(mesa);
    }

    /// <summary>Depois de gravar: avisa as telas abertas e devolve a mesa montada.</summary>
    private async Task<MesaServiceModel> AvisarEMontarAsync(Mesa mesa)
    {
        _canal.Avisar(mesa.IdCampanha);
        return await MontarAsync(mesa);
    }

    /// <summary>Monta o estado completo da mesa (para o mestre). Também usado pela visão do jogador.</summary>
    public async Task<MesaServiceModel> MontarAsync(Mesa mesa)
    {
        var participantes = await _participanteRepository.ListarPorMesaAsync(mesa.Id);
        var condicoes = (await _condicaoRepository.ListarPorMesaAsync(mesa.Id))
            .GroupBy(c => c.IdParticipante)
            .ToDictionary(g => g.Key, g => g.Select(c => c.Nome).ToList());

        // Retrato = a primeira imagem do registro; o tipo vem junto da mesma consulta (sem o conteúdo).
        var imagens = (await _imagemRepository.ListarPorCampanhaAsync(mesa.IdCampanha)).ToList();
        var retratos = imagens.GroupBy(i => i.IdRegistro).ToDictionary(g => g.Key, g => g.OrderBy(i => i.Ordem).ThenBy(i => i.Id).First().Id);
        var tipos = await TiposDosRegistrosAsync(mesa.IdCampanha, participantes);

        return new MesaServiceModel
        {
            Id = mesa.Id,
            IdCampanha = mesa.IdCampanha,
            Nome = mesa.Nome,
            IdImagemRegistro = mesa.IdImagemRegistro,
            TamanhoCelula = mesa.TamanhoCelula,
            DeslocamentoX = mesa.DeslocamentoX,
            DeslocamentoY = mesa.DeslocamentoY,
            GradeVisivel = mesa.GradeVisivel,
            MovimentoLivre = mesa.MovimentoLivre,
            Nevoa = mesa.Nevoa,
            Clima = mesa.Clima,
            CelulasReveladas = (await _celulaRepository.ListarPorMesaAsync(mesa.Id))
                .Select(c => new CelulaServiceModel { Coluna = c.Coluna, Linha = c.Linha }).ToList(),
            Ativa = mesa.Ativa,
            EmCombate = mesa.EmCombate,
            Rodada = mesa.Rodada,
            IdParticipanteVez = mesa.IdParticipanteVez,
            Participantes = RegrasMesa.OrdenarIniciativa(participantes).Select(p => new ParticipanteServiceModel
            {
                Id = p.Id,
                IdRegistro = p.IdRegistro,
                IdTipoRegistro = p.IdRegistro.HasValue && tipos.TryGetValue(p.IdRegistro.Value, out var tipo) ? tipo : null,
                IdImagemRetrato = p.IdRegistro.HasValue && retratos.TryGetValue(p.IdRegistro.Value, out var retrato) ? retrato : null,
                Nome = p.Nome,
                Jogador = p.Jogador,
                Iniciativa = p.Iniciativa,
                BonusIniciativa = p.BonusIniciativa,
                Ca = p.Ca,
                PvAtual = p.PvAtual,
                PvMaximo = p.PvMaximo,
                Coluna = p.Coluna,
                Linha = p.Linha,
                Tamanho = p.Tamanho,
                Visivel = p.Visivel,
                Ordem = p.Ordem,
                Condicoes = condicoes.TryGetValue(p.Id, out var lista) ? lista : [],
            }).ToList(),
        };
    }

    private async Task<Dictionary<int, int>> TiposDosRegistrosAsync(int idCampanha, IEnumerable<ParticipanteMesa> participantes)
    {
        if (!participantes.Any(p => p.IdRegistro.HasValue))
            return [];
        var registros = await _registroRepository.ListarAsync(idCampanha, new FiltroRegistro());
        return registros.ToDictionary(r => r.Id, r => r.IdTipoRegistro);
    }

    private async Task ConferirImagemDaCampanhaAsync(int idCampanha, int? idImagem)
    {
        if (idImagem is null)
            return;
        var imagens = await _imagemRepository.ListarPorCampanhaAsync(idCampanha);
        if (!imagens.Any(i => i.Id == idImagem))
            throw new BusinessRuleException(BusinessErrors.ImagemNaoEncontrada);
    }

    /// <summary>Mesa inexistente ou de campanha de outro usuário → MesaNaoEncontrada (404).</summary>
    private async Task<Mesa> ObterMesaDoUsuarioOuLancarAsync(int idUsuario, int id)
    {
        var mesa = await _mesaRepository.ObterAsync(id) ?? throw new BusinessRuleException(BusinessErrors.MesaNaoEncontrada);
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

    private async Task<(ParticipanteMesa Participante, Mesa Mesa)> ObterParticipanteDoUsuarioOuLancarAsync(int idUsuario, int id)
    {
        var participante = await _participanteRepository.ObterAsync(id)
                           ?? throw new BusinessRuleException(BusinessErrors.ParticipanteNaoEncontrado);
        try
        {
            return (participante, await ObterMesaDoUsuarioOuLancarAsync(idUsuario, participante.IdMesa));
        }
        catch (BusinessRuleException e) when (e.Code == BusinessErrors.MesaNaoEncontrada.Code)
        {
            throw new BusinessRuleException(BusinessErrors.ParticipanteNaoEncontrado);
        }
    }

    /// <summary>Uma linha no diário da mesa (para o resumo da sessão).</summary>
    private Task RegistrarAsync(int idMesa, string tipo, string texto) =>
        _eventoRepository.InserirAsync(new EventoMesa { IdMesa = idMesa, Tipo = tipo, Texto = texto.Length > 300 ? texto[..297] + "…" : texto, Data = Agora() });

    private static DateTime Agora()
    {
        var agora = DateTime.Now;
        return new DateTime(agora.Year, agora.Month, agora.Day, agora.Hour, agora.Minute, agora.Second, DateTimeKind.Unspecified);
    }
}
