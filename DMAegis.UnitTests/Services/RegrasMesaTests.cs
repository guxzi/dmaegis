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
}
