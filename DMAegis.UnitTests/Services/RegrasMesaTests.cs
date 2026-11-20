using DMAegis.Model;
using DMAegis.Services;

namespace DMAegis.UnitTests.Services;

public class RegrasMesaTests
{
    private static ParticipanteMesa P(int id, int? ini, int bonus = 0, bool visivel = true, int? pv = null, int? pvMax = null, int ordem = 0) =>
        new() { Id = id, Nome = $"P{id}", Iniciativa = ini, BonusIniciativa = bonus, Visivel = visivel, PvAtual = pv, PvMaximo = pvMax, Ordem = ordem };

    [Fact]
    public void OrdenarIniciativa_MaiorPrimeiro_DesempatePorBonusEOrdem_SemIniciativaNoFim()
    {
        var ordem = RegrasMesa.OrdenarIniciativa([P(1, 12), P(2, null), P(3, 18), P(4, 12, bonus: 3), P(5, 12, bonus: 3, ordem: -1)]);
        Assert.Equal([3, 5, 4, 1, 2], ordem.Select(p => p.Id));
    }

    [Fact]
    public void ProximaVez_PulaOcultoECaido_ESomaRodadaAoDarAVolta()
    {
        var ordem = new List<ParticipanteMesa> { P(1, 20), P(2, 15, visivel: false), P(3, 10, pv: 0, pvMax: 7), P(4, 5) };

        Assert.Equal((4, 1), RegrasMesa.ProximaVez(ordem, 1, 1));
        Assert.Equal((1, 2), RegrasMesa.ProximaVez(ordem, 4, 1));
    }

    [Fact]
    public void ProximaVez_SemVezAtual_ComecaNoPrimeiroSemSomarRodada()
    {
        Assert.Equal((1, 1), RegrasMesa.ProximaVez([P(1, 20), P(2, 10)], null, 1));
    }

    [Fact]
    public void ProximaVez_NinguemPodeAgir_MantemTudo()
    {
        Assert.Equal((2, 3), RegrasMesa.ProximaVez([P(1, 20, visivel: false), P(2, 10, pv: 0, pvMax: 5)], 2, 3));
    }

    [Theory]
    [InlineData(10, 10, "Ileso")]
    [InlineData(9, 10, "Ferido")]
    [InlineData(5, 10, "Sangrando")]
    [InlineData(0, 10, "Caído")]
    [InlineData(-3, 10, "Caído")]
    [InlineData(5, null, null)]
    public void EstadoSaude_EmPalavras(int? atual, int? maximo, string? esperado)
    {
        Assert.Equal(esperado, RegrasMesa.EstadoSaude(atual, maximo));
    }

    [Fact]
    public void NomesNumerados_UmSoNomePuro_VariosNumerados_ContinuaDoMaior()
    {
        Assert.Equal(["Goblin"], RegrasMesa.NomesNumerados("Goblin", 1, []));
        Assert.Equal(["Goblin 1", "Goblin 2"], RegrasMesa.NomesNumerados("Goblin", 2, []));
        Assert.Equal(["Goblin 3", "Goblin 4"], RegrasMesa.NomesNumerados("Goblin", 2, ["Goblin 1", "Goblin 2", "Lobo"]));
        Assert.Equal(["Goblin 2"], RegrasMesa.NomesNumerados("Goblin", 1, ["Goblin"]));
    }

    [Theory]
    [InlineData("15", 15)]
    [InlineData("15 (armadura natural)", 15)]
    [InlineData("+2", 2)]
    [InlineData("-1", -1)]
    [InlineData("9 m", 9)]
    [InlineData("alto", null)]
    [InlineData("", null)]
    public void NumeroDoAtributo_LeOComeco(string valor, int? esperado)
    {
        Assert.Equal(esperado, RegrasMesa.NumeroDoAtributo(valor));
    }

    [Theory]
    [InlineData("1d20+5", "1d20+5 → (20) + 5 = 25", "critico")]
    [InlineData("1d20", "1d20 → (1) = 1", "falha")]
    [InlineData("2d20kh1+3", "2d20kh1+3 → ([4], 20) + 3 = 23", "critico")]
    [InlineData("2d20kl1", "2d20kl1 → (1, [17]) = 1", "falha")]
    [InlineData("1d20", "1d20 → (12) = 12", null)]
    [InlineData("2d20", "2d20 → (20, 20) = 40", null)]
    [InlineData("1d12", "1d12 → (1) = 1", null)]
    [InlineData("1d20+1d4", "1d20+1d4 → (20) + (2) = 22", null)]
    [InlineData("1d20 (iniciativa)", "1d20 → (20) = 20", "critico")]
    public void CriticoDaRolagem_IgualAoFront(string expressao, string detalhe, string? esperado) =>
        Assert.Equal(esperado, RegrasMesa.CriticoDaRolagem(expressao, detalhe));

    [Fact]
    public void ResumoMesa_JuntaCombateDadosEEntregas()
    {
        var dia = new DateTime(2026, 11, 21, 20, 0, 0);
        var texto = ResumoMesaService.Montar("Ponte",
            [
                new EventoMesa { Texto = "O combate começou. Ordem: Lia (18), Goblin (7).", Data = dia.AddMinutes(5) },
                new EventoMesa { Texto = "Goblin caiu.", Data = dia.AddMinutes(12) },
            ],
            [
                new RolagemMesa { Autor = "Lia", Expressao = "1d20+5", Total = 25, Detalhe = "1d20+5 → (20) + 5 = 25", Data = dia.AddMinutes(10) },
                new RolagemMesa { Autor = "Mestre", Expressao = "1d20", Total = 1, Detalhe = "1d20 → (1) = 1", Secreta = true, Data = dia.AddMinutes(11) },
                new RolagemMesa { Autor = "Lia", Expressao = "1d6", Total = 4, Detalhe = "1d6 → (4) = 4", Data = dia.AddMinutes(11) },
            ],
            [new Entrega { Titulo = "Carta do prefeito", Data = dia.AddMinutes(40) }]);

        Assert.StartsWith("Mesa \"Ponte\", 21/11, das 20:05 às 20:40.", texto);
        Assert.Contains("- 20:12: Goblin caiu.", texto);
        Assert.Contains("- 20:10: Lia tirou 20 natural (1d20+5 = 25).", texto);
        Assert.Contains("Mestre tirou 1 natural (1d20 = 1) (secreta).", texto);
        Assert.Contains("- 3 rolagens no total (Lia: 2, Mestre: 1).", texto);
        Assert.Contains("- 20:40: Carta do prefeito.", texto);
        Assert.Equal("Nada registrado na mesa \"Ponte\" neste período.", ResumoMesaService.Montar("Ponte", [], [], []));
        var uma = ResumoMesaService.Montar("Ponte", [], [new RolagemMesa { Autor = "Lia", Expressao = "1d6", Total = 2, Detalhe = "1d6 → (2) = 2", Data = dia }], []);
        Assert.StartsWith("Mesa \"Ponte\", 21/11, às 20:00.", uma);
        Assert.Contains("- 1 rolagem no total (Lia: 1).", uma);
    }
}
