using System.Globalization;
using System.Text;
using DMAegis.Model;
using DMAegis.Repository;
using DMAegis.Services.Exceptions;

namespace DMAegis.Services;

public interface IResumoMesaService
{
    /// <summary>Rascunho do resumo da sessão: o que aconteceu na mesa nas últimas `horas` (combates, quedas, dados, entregas).</summary>
    Task<string> GerarAsync(int idUsuario, int idMesa, int horas);
}

/// <summary>Monta o rascunho do resumo da sessão a partir do diário da mesa, das rolagens e das entregas.</summary>
public class ResumoMesaService : IResumoMesaService
{
    private readonly IMesaRepository _mesaRepository;
    private readonly ICampanhaService _campanhaService;
    private readonly IEventoMesaRepository _eventoRepository;
    private readonly IRolagemMesaRepository _rolagemRepository;
    private readonly IEntregaRepository _entregaRepository;

    public ResumoMesaService(IMesaRepository mesaRepository, ICampanhaService campanhaService, IEventoMesaRepository eventoRepository,
        IRolagemMesaRepository rolagemRepository, IEntregaRepository entregaRepository)
    {
        _mesaRepository = mesaRepository;
        _campanhaService = campanhaService;
        _eventoRepository = eventoRepository;
        _rolagemRepository = rolagemRepository;
        _entregaRepository = entregaRepository;
    }

    public async Task<string> GerarAsync(int idUsuario, int idMesa, int horas)
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

        var desde = DateTime.Now.AddHours(-Math.Clamp(horas, 1, 48));
        var eventos = (await _eventoRepository.ListarDesdeAsync(idMesa, desde)).ToList();
        var rolagens = (await _rolagemRepository.ListarDesdeAsync(idMesa, desde)).ToList();
        var entregas = (await _entregaRepository.ListarPorCampanhaAsync(mesa.IdCampanha)).Where(e => e.Data >= desde).OrderBy(e => e.Data).ToList();
        return Montar(mesa.Nome, eventos, rolagens, entregas);
    }

    private static readonly CultureInfo Br = CultureInfo.GetCultureInfo("pt-BR");
    private static string Hora(DateTime d) => d.ToString("HH:mm", Br);

    /// <summary>O texto do resumo. Sem nada no período, uma frase dizendo isso.</summary>
    public static string Montar(string nomeMesa, IReadOnlyList<EventoMesa> eventos, IReadOnlyList<RolagemMesa> rolagens, IReadOnlyList<Entrega> entregas)
    {
        var datas = eventos.Select(e => e.Data).Concat(rolagens.Select(r => r.Data)).Concat(entregas.Select(e => e.Data)).ToList();
        if (datas.Count == 0)
            return $"Nada registrado na mesa \"{nomeMesa}\" neste período.";

        var texto = new StringBuilder();
        var (inicio, fim) = (Hora(datas.Min()), Hora(datas.Max()));
        texto.Append($"Mesa \"{nomeMesa}\", {datas.Min().ToString("dd/MM", Br)}, {(inicio == fim ? $"às {inicio}" : $"das {inicio} às {fim}")}.");

        if (eventos.Count > 0)
        {
            texto.Append("\n\nCombate\n");
            foreach (var e in eventos)
                texto.Append($"- {Hora(e.Data)}: {e.Texto}\n");
            texto.Length--;
        }

        var momentos = rolagens
            .Select(r => (Rolagem: r, Critico: RegrasMesa.CriticoDaRolagem(r.Expressao, r.Detalhe)))
            .Where(x => x.Critico is not null)
            .ToList();
        if (rolagens.Count > 0)
        {
            texto.Append("\n\nDados\n");
            foreach (var (r, critico) in momentos)
            {
                var o = critico == "critico" ? "20 natural" : "1 natural";
                var secreta = r.Secreta ? " (secreta)" : string.Empty;
                texto.Append($"- {Hora(r.Data)}: {r.Autor} tirou {o} ({r.Expressao} = {r.Total}){secreta}.\n");
            }
            var porAutor = rolagens.GroupBy(r => r.Autor).OrderByDescending(g => g.Count()).Select(g => $"{g.Key}: {g.Count()}");
            texto.Append($"- {rolagens.Count} {(rolagens.Count == 1 ? "rolagem" : "rolagens")} no total ({string.Join(", ", porAutor)}).");
        }

        if (entregas.Count > 0)
        {
            texto.Append("\n\nEntregas aos jogadores\n");
            foreach (var e in entregas)
                texto.Append($"- {Hora(e.Data)}: {e.Titulo}{(e.IdRegistroDestino.HasValue ? " (só para um PJ)" : string.Empty)}.\n");
            texto.Length--;
        }
        return texto.ToString();
    }
}
