using DMAegis.Model;
using DMAegis.Repository;
using DMAegis.Services.Exceptions;

namespace DMAegis.Services;

// O que os jogadores veem (o filtro) e a névoa de guerra.
public partial class MesaService
{
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
}
