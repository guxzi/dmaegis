using DMAegis.Model;
using DMAegis.Repository;
using DMAegis.Services.Exceptions;

namespace DMAegis.Services;

// Combate: começar, passar o turno e encerrar (com o diário da mesa).
public partial class MesaService
{
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
}
