using DMAegis.Model;
using DMAegis.Repository;
using DMAegis.Services.Exceptions;

namespace DMAegis.Services;

// Sinais passageiros do mestre (ping, reação, arrasto ao vivo, pedir iniciativa).
public partial class MesaService
{
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
}
