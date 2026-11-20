using DMAegis.Model;
using DMAegis.Repository;
using DMAegis.Services.Exceptions;

namespace DMAegis.Services;

// Fichas da mesa: adicionar, editar, mover e remover.
public partial class MesaService
{
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
}
