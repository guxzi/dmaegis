using DMAegis.Model;

namespace DMAegis.Services;

/// <summary>Regras puras da mesa: ordem da iniciativa, passagem de turno, saúde em palavras e nomes numerados.</summary>
public static class RegrasMesa
{
    public const string Ileso = "Ileso";
    public const string Ferido = "Ferido";
    public const string Sangrando = "Sangrando";
    public const string Caido = "Caído";

    public const string EventoCombateInicio = "combate-inicio";
    public const string EventoCombateFim = "combate-fim";
    public const string EventoCaiu = "caiu";

    /// <summary>
    /// Crítico ("critico") ou falha ("falha") de uma rolagem com um único d20 que conta (1d20, 2d20kh1, 2d20kl1, com bônus).
    /// O detalhe vem do navegador: "1d20+5 → (20) + 5 = 25"; dado entre colchetes foi descartado. Igual ao efeitos.js do front.
    /// </summary>
    public static string? CriticoDaRolagem(string? expressao, string? detalhe)
    {
        var termos = System.Text.RegularExpressions.Regex.Matches((expressao ?? string.Empty).ToLowerInvariant(), @"(\d*)d(\d+)(k[hl](\d+))?");
        if (termos.Count != 1 || termos[0].Groups[2].Value != "20")
            return null;
        var t = termos[0];
        var contam = t.Groups[4].Success ? int.Parse(t.Groups[4].Value) : (t.Groups[1].Value.Length > 0 ? int.Parse(t.Groups[1].Value) : 1);
        if (contam != 1)
            return null;
        var grupo = System.Text.RegularExpressions.Regex.Match(detalhe ?? string.Empty, @"\(([^)]*)\)");
        if (!grupo.Success)
            return null;
        var mantido = grupo.Groups[1].Value.Split(',').Select(v => v.Trim()).FirstOrDefault(v => !v.StartsWith('['));
        return mantido switch { "20" => "critico", "1" => "falha", _ => null };
    }

    /// <summary>Climas que a mesa pode mostrar por cima do mapa (nulo = nenhum).</summary>
    public static readonly string[] Climas = ["chuva", "neve", "neblina", "brasas"];

    /// <summary>Reações rápidas dos jogadores (sobem flutuando da ficha de quem reagiu).</summary>
    public static readonly string[] Reacoes = ["😱", "🔥", "⭐", "💀", "😂", "👏", "❤️", "🎲"];

    public const string SinalPing = "ping";
    public const string SinalReacao = "reacao";
    public const string SinalArrasto = "arrasto";
    public const string SinalArrastoFim = "arrasto-fim";
    public const string SinalPedirIniciativa = "pedir-iniciativa";
    public static readonly string[] TiposSinal = [SinalPing, SinalReacao, SinalArrasto, SinalArrastoFim, SinalPedirIniciativa];

    /// <summary>
    /// Ordem do combate: iniciativa maior primeiro (sem iniciativa vai para o fim),
    /// desempate pelo bônus, depois pela ordem manual e pelo id.
    /// </summary>
    public static List<ParticipanteMesa> OrdenarIniciativa(IEnumerable<ParticipanteMesa> participantes) =>
        participantes
            .OrderBy(p => p.Iniciativa.HasValue ? 0 : 1)
            .ThenByDescending(p => p.Iniciativa ?? int.MinValue)
            .ThenByDescending(p => p.BonusIniciativa)
            .ThenBy(p => p.Ordem)
            .ThenBy(p => p.Id)
            .ToList();

    /// <summary>Quem pode ter a vez: visível e de pé (sem PV controlado ou com PV acima de 0).</summary>
    public static bool PodeAgir(ParticipanteMesa p) => p.Visivel && !(p.PvMaximo.HasValue && (p.PvAtual ?? 0) <= 0);

    /// <summary>
    /// Próxima vez a partir da atual. Volta ao começo da lista somando 1 à rodada.
    /// Sem ninguém que possa agir, mantém a vez e a rodada.
    /// </summary>
    public static (int? IdVez, int Rodada) ProximaVez(IReadOnlyList<ParticipanteMesa> ordem, int? idVezAtual, int rodada)
    {
        if (!ordem.Any(PodeAgir))
            return (idVezAtual, rodada);

        var indice = idVezAtual is null ? -1 : ordem.ToList().FindIndex(p => p.Id == idVezAtual);
        for (var passo = 1; passo <= ordem.Count; passo++)
        {
            var proximo = indice + passo;
            var deuVolta = proximo >= ordem.Count;
            var candidato = ordem[proximo % ordem.Count];
            if (!PodeAgir(candidato))
                continue;
            var voltas = deuVolta && indice >= 0 ? 1 : 0;
            return (candidato.Id, rodada + voltas);
        }
        return (idVezAtual, rodada);
    }

    /// <summary>Primeira vez do combate: o primeiro da ordem que pode agir.</summary>
    public static int? PrimeiraVez(IReadOnlyList<ParticipanteMesa> ordem) => ordem.FirstOrDefault(PodeAgir)?.Id;

    /// <summary>Saúde em palavras, para os jogadores não verem o PV dos monstros. Nulo = sem PV controlado.</summary>
    public static string? EstadoSaude(int? pvAtual, int? pvMaximo)
    {
        if (pvMaximo is null or <= 0)
            return null;
        var atual = pvAtual ?? 0;
        if (atual <= 0) return Caido;
        if (atual * 2 <= pvMaximo) return Sangrando;
        return atual < pvMaximo ? Ferido : Ileso;
    }

    /// <summary>
    /// Nomes para "Goblin × 3": com um só, o nome puro (se ainda não existe); com vários, numera
    /// continuando do maior número que já existe ("Goblin 2" na mesa → Goblin 3, 4…).
    /// </summary>
    public static List<string> NomesNumerados(string nomeBase, int quantidade, IEnumerable<string> existentes)
    {
        var lista = existentes.ToList();
        var prefixo = nomeBase + " ";
        var maior = lista
            .Where(n => n.StartsWith(prefixo, StringComparison.Ordinal))
            .Select(n => int.TryParse(n[prefixo.Length..], out var numero) ? numero : 0)
            .DefaultIfEmpty(0)
            .Max();

        if (quantidade == 1 && maior == 0 && !lista.Contains(nomeBase))
            return [nomeBase];

        var inicio = Math.Max(maior, lista.Contains(nomeBase) ? 1 : 0) + 1;
        return Enumerable.Range(inicio, quantidade).Select(n => $"{nomeBase} {n}").ToList();
    }

    /// <summary>Lê um número inteiro do começo do valor de um atributo ("15 (armadura natural)" → 15).</summary>
    public static int? NumeroDoAtributo(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return null;
        var texto = valor.Trim();
        var fim = 0;
        if (fim < texto.Length && (texto[fim] == '-' || texto[fim] == '+')) fim++;
        while (fim < texto.Length && char.IsDigit(texto[fim])) fim++;
        return int.TryParse(texto[..fim], out var numero) ? numero : null;
    }
}
