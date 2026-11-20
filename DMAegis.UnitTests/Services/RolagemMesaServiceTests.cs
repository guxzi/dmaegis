using DMAegis.Model;
using DMAegis.Repository;
using DMAegis.Services;
using DMAegis.Services.Exceptions;
using Moq;

namespace DMAegis.UnitTests.Services;

public class RolagemMesaServiceTests
{
    private readonly Mock<IRolagemMesaRepository> _rolagens = new();
    private readonly Mock<IMesaRepository> _mesas = new();
    private readonly Mock<ICampanhaService> _campanhas = new();
    private readonly Mock<IJogadorService> _jogadores = new();
    private readonly Mock<ICanalMesa> _canal = new();
    private readonly Mock<IParticipanteMesaRepository> _participantes = new();
    private readonly JogadorLogadoServiceModel _lilica = new() { IdCampanha = 10, IdRegistro = 100, NomePersonagem = "Lilica Lovejoy" };

    public RolagemMesaServiceTests()
    {
        _campanhas.Setup(c => c.ObterDoUsuarioOuLancarAsync(1, 10)).ReturnsAsync(new Campanha { Id = 10, IdUsuario = 1 });
        _campanhas.Setup(c => c.ObterDoUsuarioOuLancarAsync(1, 20)).ThrowsAsync(new BusinessRuleException(BusinessErrors.CampanhaNaoEncontrada));
        _mesas.Setup(m => m.ObterAsync(5)).ReturnsAsync(new Mesa { Id = 5, IdCampanha = 10 });
        _mesas.Setup(m => m.ObterAsync(6)).ReturnsAsync(new Mesa { Id = 6, IdCampanha = 20 });
        _mesas.Setup(m => m.ObterAtivaAsync(10)).ReturnsAsync(new Mesa { Id = 5, IdCampanha = 10 });
    }

    private RolagemMesaService CriarService() => new(_rolagens.Object, _mesas.Object, _campanhas.Object, _jogadores.Object, _canal.Object, _participantes.Object);

    private static readonly RolarServiceModel D20 = new() { Expressao = "1d20+5", Total = 19, Detalhe = "1d20+5 → (14) + 5 = 19" };

    [Fact]
    public async Task RolarDoMestreAsync_Secreta_NaoAvisaOsJogadores()
    {
        RolagemMesa? gravada = null;
        _rolagens.Setup(r => r.InserirAsync(It.IsAny<RolagemMesa>())).Callback<RolagemMesa>(r => gravada = r).ReturnsAsync(3);

        await CriarService().RolarDoMestreAsync(1, 5, new RolarServiceModel { Expressao = "1d20", Total = 7, Detalhe = "1d20 → (7) = 7", Secreta = true });

        Assert.Equal(("Mestre", true, true), (gravada!.Autor, gravada.Mestre, gravada.Secreta));
        _canal.Verify(c => c.Avisar(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task RolarDoMestreAsync_MesaAlheia_Codigo17()
    {
        var erro = await Assert.ThrowsAsync<BusinessRuleException>(() => CriarService().RolarDoMestreAsync(1, 6, D20));
        Assert.Equal(BusinessErrors.MesaNaoEncontrada.Code, erro.Code);
    }

    [Fact]
    public async Task RolarDoJogadorAsync_UsaONomeDoPersonagem_NuncaSecreta_EAvisa()
    {
        RolagemMesa? gravada = null;
        _rolagens.Setup(r => r.InserirAsync(It.IsAny<RolagemMesa>())).Callback<RolagemMesa>(r => gravada = r).ReturnsAsync(4);

        await CriarService().RolarDoJogadorAsync(_lilica, new RolarServiceModel { Expressao = "1d20", Total = 12, Detalhe = "1d20 → (12) = 12", Secreta = true });

        Assert.Equal(("Lilica Lovejoy", false, false, 5), (gravada!.Autor, gravada.Mestre, gravada.Secreta, gravada.IdMesa));
        _jogadores.Verify(j => j.ConferirAcessoAsync(_lilica), Times.Once);
        _canal.Verify(c => c.Avisar(10), Times.Once);
    }

    [Fact]
    public async Task ListarDoJogadorAsync_SemSecretas()
    {
        _rolagens.Setup(r => r.ListarRecentesAsync(5, false, RolagemMesaService.Limite)).ReturnsAsync([new RolagemMesa { Id = 1, Autor = "Mestre" }]);

        var lista = (await CriarService().ListarDoJogadorAsync(_lilica)).ToList();

        Assert.Single(lista);
        _rolagens.Verify(r => r.ListarRecentesAsync(5, true, It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ListarDoJogadorAsync_SemMesaAtiva_ListaVazia()
    {
        _mesas.Setup(m => m.ObterAtivaAsync(10)).ReturnsAsync((Mesa?)null);
        Assert.Empty(await CriarService().ListarDoJogadorAsync(_lilica));
    }

    [Fact]
    public async Task RolarIniciativaDoJogadorAsync_GravaNaFicha_ENaoDeixaRolarDeNovoNoCombate()
    {
        _participantes.Setup(p => p.ListarPorMesaAsync(5)).ReturnsAsync(
        [
            new ParticipanteMesa { Id = 1, IdMesa = 5, IdRegistro = 100, Jogador = true, Visivel = true },
            new ParticipanteMesa { Id = 2, IdMesa = 5, IdRegistro = 101, Visivel = true },
        ]);
        RolagemMesa? gravada = null;
        _rolagens.Setup(r => r.InserirAsync(It.IsAny<RolagemMesa>())).Callback<RolagemMesa>(r => gravada = r).ReturnsAsync(9);

        var rolagem = await CriarService().RolarIniciativaDoJogadorAsync(_lilica, new RolarServiceModel { Expressao = "1d20+2", Total = 17, Detalhe = "1d20+2 → (15) + 2 = 17" });

        _participantes.Verify(p => p.AtualizarIniciativaAsync(1, 17), Times.Once);
        Assert.Equal("1d20+2 (iniciativa)", gravada!.Expressao);
        Assert.Equal("Iniciativa: 1d20+2 → (15) + 2 = 17", gravada.Detalhe);
        Assert.False(rolagem.Secreta);

        _mesas.Setup(m => m.ObterAtivaAsync(10)).ReturnsAsync(new Mesa { Id = 5, IdCampanha = 10, EmCombate = true });
        _participantes.Setup(p => p.ListarPorMesaAsync(5)).ReturnsAsync(
            [new ParticipanteMesa { Id = 1, IdMesa = 5, IdRegistro = 100, Jogador = true, Visivel = true, Iniciativa = 17 }]);
        var erro = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CriarService().RolarIniciativaDoJogadorAsync(_lilica, new RolarServiceModel { Expressao = "1d20", Total = 3, Detalhe = "1d20 → (3) = 3" }));
        Assert.Equal(BusinessErrors.IniciativaJaDefinida.Code, erro.Code);
    }
}
