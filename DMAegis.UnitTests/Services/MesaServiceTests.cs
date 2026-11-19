using DMAegis.Model;
using DMAegis.Repository;
using DMAegis.Services;
using DMAegis.Services.Exceptions;
using Moq;

namespace DMAegis.UnitTests.Services;

public class MesaServiceTests
{
    private readonly Mock<IMesaRepository> _mesas = new();
    private readonly Mock<IParticipanteMesaRepository> _participantes = new();
    private readonly Mock<ICondicaoParticipanteRepository> _condicoes = new();
    private readonly Mock<IRegistroRepository> _registros = new();
    private readonly Mock<IAtributoRegistroRepository> _atributos = new();
    private readonly Mock<IImagemRegistroRepository> _imagens = new();
    private readonly Mock<ICampanhaService> _campanhas = new();
    private readonly Mock<ICanalMesa> _canal = new();
    private readonly Mock<ICelulaReveladaRepository> _celulas = new();

    private readonly List<ParticipanteMesa> _naMesa = [];

    public MesaServiceTests()
    {
        _campanhas.Setup(c => c.ObterDoUsuarioOuLancarAsync(1, 10)).ReturnsAsync(new Campanha { Id = 10, IdUsuario = 1 });
        _campanhas.Setup(c => c.ObterDoUsuarioOuLancarAsync(1, 20))
            .ThrowsAsync(new BusinessRuleException(BusinessErrors.CampanhaNaoEncontrada));
        _mesas.Setup(m => m.ObterAsync(5)).ReturnsAsync(() => new Mesa { Id = 5, IdCampanha = 10, Nome = "Ponte", EmCombate = true, Rodada = 1 });
        _mesas.Setup(m => m.ObterAsync(6)).ReturnsAsync(new Mesa { Id = 6, IdCampanha = 20, Nome = "Alheia" });
        _participantes.Setup(p => p.ListarPorMesaAsync(5)).ReturnsAsync(() => _naMesa.ToList());
        _condicoes.Setup(c => c.ListarPorMesaAsync(5)).ReturnsAsync([]);
        _imagens.Setup(i => i.ListarPorCampanhaAsync(10)).ReturnsAsync(
        [
            new ImagemDaCampanha { Id = 300, IdRegistro = 100, Ordem = 0 },
            new ImagemDaCampanha { Id = 301, IdRegistro = 100, Ordem = 1 },
        ]);
        _registros.Setup(r => r.ListarAsync(10, It.IsAny<FiltroRegistro>())).ReturnsAsync(
        [
            new Registro { Id = 100, IdCampanha = 10, IdTipoRegistro = 2, Nome = "Lilica Lovejoy" },
            new Registro { Id = 101, IdCampanha = 10, IdTipoRegistro = 3, Nome = "Goblin" },
        ]);
    }

    private MesaService CriarService() => new(_mesas.Object, _participantes.Object, _condicoes.Object,
        _registros.Object, _atributos.Object, _imagens.Object, _campanhas.Object, _canal.Object, _celulas.Object);

    private static async Task<int> CodigoAsync(Func<Task> acao) =>
        (await Assert.ThrowsAsync<BusinessRuleException>(acao)).Code;

    [Fact]
    public async Task ObterAsync_MesaDeOutraCampanha_Codigo17()
    {
        Assert.Equal(BusinessErrors.MesaNaoEncontrada.Code, await CodigoAsync(() => CriarService().ObterAsync(1, 6)));
        Assert.Equal(BusinessErrors.MesaNaoEncontrada.Code, await CodigoAsync(() => CriarService().ObterAsync(1, 99)));
    }

    [Fact]
    public async Task InserirAsync_PrimeiraMesaNasceAtiva_EImagemPrecisaSerDaCampanha()
    {
        _mesas.Setup(m => m.ListarPorCampanhaAsync(10)).ReturnsAsync([]);
        _mesas.Setup(m => m.InserirAsync(It.IsAny<Mesa>())).ReturnsAsync(7);
        _participantes.Setup(p => p.ListarPorMesaAsync(7)).ReturnsAsync([]);
        _condicoes.Setup(c => c.ListarPorMesaAsync(7)).ReturnsAsync([]);

        var mesa = await CriarService().InserirAsync(1, 10, new SalvarMesaServiceModel { Nome = " Caverna ", IdImagemRegistro = 300 });

        Assert.Equal((7, "Caverna", true), (mesa.Id, mesa.Nome, mesa.Ativa));
        Assert.Equal(BusinessErrors.ImagemNaoEncontrada.Code,
            await CodigoAsync(() => CriarService().InserirAsync(1, 10, new SalvarMesaServiceModel { Nome = "X", IdImagemRegistro = 999 })));
    }

    [Fact]
    public async Task InserirParticipantesAsync_DoCaderno_NumeraELeCaPvDosAtributos()
    {
        _naMesa.Add(new ParticipanteMesa { Id = 1, IdMesa = 5, Nome = "Goblin 1", Ordem = 0 });
        _registros.Setup(r => r.ObterAsync(101)).ReturnsAsync(new Registro { Id = 101, IdCampanha = 10, IdTipoRegistro = 3, Nome = "Goblin" });
        _atributos.Setup(a => a.ListarPorRegistroAsync(101)).ReturnsAsync(
        [
            new AtributoRegistro { Chave = "CA", Valor = "15 (armadura de couro)" },
            new AtributoRegistro { Chave = "PV", Valor = "7" },
        ]);
        var inseridos = new List<ParticipanteMesa>();
        _participantes.Setup(p => p.InserirAsync(It.IsAny<ParticipanteMesa>())).Callback<ParticipanteMesa>(inseridos.Add).ReturnsAsync(0);

        await CriarService().InserirParticipantesAsync(1, 5, new InserirParticipantesServiceModel { IdRegistro = 101, Quantidade = 2 });

        Assert.Equal(["Goblin 2", "Goblin 3"], inseridos.Select(p => p.Nome));
        Assert.All(inseridos, p => Assert.Equal((15, 7, 7, false, 101), (p.Ca, p.PvAtual, p.PvMaximo, p.Jogador, p.IdRegistro)));
        Assert.Equal([1, 2], inseridos.Select(p => p.Ordem));
    }

    [Fact]
    public async Task InserirParticipantesAsync_RegistroDeOutraCampanha_Codigo7()
    {
        _registros.Setup(r => r.ObterAsync(200)).ReturnsAsync(new Registro { Id = 200, IdCampanha = 20, Nome = "Alheio" });
        Assert.Equal(BusinessErrors.RegistroNaoEncontrado.Code,
            await CodigoAsync(() => CriarService().InserirParticipantesAsync(1, 5, new InserirParticipantesServiceModel { IdRegistro = 200 })));
    }

    [Fact]
    public async Task InserirParticipantesAsync_TodosJogadores_SoOsQueFaltam()
    {
        _naMesa.Add(new ParticipanteMesa { Id = 1, IdMesa = 5, IdRegistro = 102, Nome = "Bardo", Jogador = true });
        _registros.Setup(r => r.ListarAsync(10, It.Is<FiltroRegistro>(f => f.IdTipoRegistro == 2))).ReturnsAsync(
        [
            new Registro { Id = 100, IdCampanha = 10, IdTipoRegistro = 2, Nome = "Lilica Lovejoy" },
            new Registro { Id = 102, IdCampanha = 10, IdTipoRegistro = 2, Nome = "Bardo" },
        ]);
        _atributos.Setup(a => a.ListarPorRegistroAsync(It.IsAny<int>())).ReturnsAsync([]);
        var inseridos = new List<ParticipanteMesa>();
        _participantes.Setup(p => p.InserirAsync(It.IsAny<ParticipanteMesa>())).Callback<ParticipanteMesa>(inseridos.Add).ReturnsAsync(0);

        await CriarService().InserirParticipantesAsync(1, 5, new InserirParticipantesServiceModel { TodosJogadores = true });

        var lilica = Assert.Single(inseridos);
        Assert.Equal(("Lilica Lovejoy", true), (lilica.Nome, lilica.Jogador));
    }

    [Fact]
    public async Task InserirParticipantesAsync_PassaDoLimite_Codigo19()
    {
        _naMesa.AddRange(Enumerable.Range(1, 60).Select(i => new ParticipanteMesa { Id = i, Nome = $"F{i}" }));
        Assert.Equal(BusinessErrors.LimiteParticipantes.Code,
            await CodigoAsync(() => CriarService().InserirParticipantesAsync(1, 5, new InserirParticipantesServiceModel { Nome = "Mais um" })));
    }

    [Fact]
    public async Task AtualizarParticipanteAsync_PvFicaEntreZeroEOMaximo_ECondicoesSemRepetir()
    {
        _participantes.Setup(p => p.ObterAsync(1)).ReturnsAsync(new ParticipanteMesa { Id = 1, IdMesa = 5, Nome = "Goblin" });
        ParticipanteMesa? gravado = null;
        _participantes.Setup(p => p.AtualizarAsync(It.IsAny<ParticipanteMesa>())).Callback<ParticipanteMesa>(p => gravado = p);
        IEnumerable<string>? condicoes = null;
        _condicoes.Setup(c => c.SubstituirAsync(1, It.IsAny<IEnumerable<string>>())).Callback<int, IEnumerable<string>>((_, n) => condicoes = n.ToList());

        await CriarService().AtualizarParticipanteAsync(1, 1, new SalvarParticipanteServiceModel
        {
            Nome = "Goblin", PvAtual = 12, PvMaximo = 7, Condicoes = ["Caído", "caído", " ", "Envenenado"],
        });
        Assert.Equal(7, gravado!.PvAtual);
        Assert.Equal(["Caído", "Envenenado"], condicoes!);

        await CriarService().AtualizarParticipanteAsync(1, 1, new SalvarParticipanteServiceModel { Nome = "Goblin", PvAtual = -4, PvMaximo = 7 });
        Assert.Equal(0, gravado!.PvAtual);
    }

    [Fact]
    public async Task MoverParticipanteAsync_FichaDeMesaAlheia_Codigo18()
    {
        _participantes.Setup(p => p.ObterAsync(2)).ReturnsAsync(new ParticipanteMesa { Id = 2, IdMesa = 6 });
        Assert.Equal(BusinessErrors.ParticipanteNaoEncontrado.Code,
            await CodigoAsync(() => CriarService().MoverParticipanteAsync(1, 2, 3, 4)));
        _participantes.Verify(p => p.AtualizarPosicaoAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()), Times.Never);
    }

    [Fact]
    public async Task ProximoTurnoAsync_PassaAVezNaOrdemDaIniciativa()
    {
        _naMesa.AddRange(
        [
            new ParticipanteMesa { Id = 1, Nome = "Goblin", Iniciativa = 12, Visivel = true },
            new ParticipanteMesa { Id = 2, Nome = "Lilica", Iniciativa = 18, Visivel = true },
        ]);
        _mesas.Setup(m => m.ObterAsync(5)).ReturnsAsync(new Mesa { Id = 5, IdCampanha = 10, EmCombate = true, Rodada = 1, IdParticipanteVez = 2 });

        var mesa = await CriarService().ProximoTurnoAsync(1, 5);

        Assert.Equal((1, 1), (mesa.IdParticipanteVez, mesa.Rodada));
        _mesas.Verify(m => m.AtualizarCombateAsync(5, true, 1, 1));
        _canal.Verify(c => c.Avisar(10), Times.Once);
    }

    [Fact]
    public void FiltrarParaJogador_TiraOcultos_EscondePvDosMonstros_MarcaAMinha()
    {
        var mesa = new MesaServiceModel
        {
            Id = 5, IdParticipanteVez = 3,
            Participantes =
            [
                new ParticipanteServiceModel { Id = 1, IdRegistro = 100, Nome = "Lilica", Jogador = true, Visivel = true, PvAtual = 20, PvMaximo = 24, Ca = 14 },
                new ParticipanteServiceModel { Id = 2, IdRegistro = 101, Nome = "Goblin 1", Visivel = true, PvAtual = 3, PvMaximo = 7, Ca = 15, Condicoes = ["Caído"] },
                new ParticipanteServiceModel { Id = 3, IdRegistro = 101, Nome = "Goblin oculto", Visivel = false, PvAtual = 7, PvMaximo = 7 },
            ],
        };

        var visao = MesaService.FiltrarParaJogador(mesa, idRegistroJogador: 100);

        Assert.Equal([1, 2], visao.Participantes.Select(p => p.Id));
        Assert.Null(visao.IdParticipanteVez);
        var lilica = visao.Participantes[0];
        Assert.Equal((20, 24, 14, true, "Ferido"), (lilica.PvAtual, lilica.PvMaximo, lilica.Ca, lilica.EhMinha, lilica.EstadoSaude));
        var goblin = visao.Participantes[1];
        Assert.Equal(((int?)null, (int?)null, (int?)null, false, "Sangrando"), (goblin.PvAtual, goblin.PvMaximo, goblin.Ca, goblin.EhMinha, goblin.EstadoSaude));
        Assert.Equal(["Caído"], goblin.Condicoes);
    }

    [Fact]
    public async Task ObterAsync_RetratoETipoVemDoCaderno()
    {
        _naMesa.Add(new ParticipanteMesa { Id = 1, IdRegistro = 100, Nome = "Lilica Lovejoy", Jogador = true, Visivel = true });

        var mesa = await CriarService().ObterAsync(1, 5);

        var lilica = Assert.Single(mesa.Participantes);
        Assert.Equal((300, 2), (lilica.IdImagemRetrato, lilica.IdTipoRegistro));
    }

    [Fact]
    public void FiltrarParaJogador_ComNevoa_EscondeMonstroNaNevoa_MasMostraPj()
    {
        var mesa = new MesaServiceModel
        {
            Nevoa = true,
            CelulasReveladas = [new() { Coluna = 1, Linha = 1 }, new() { Coluna = 2, Linha = 1 }, new() { Coluna = 1, Linha = 2 }, new() { Coluna = 2, Linha = 2 }],
            Participantes =
            [
                new ParticipanteServiceModel { Id = 1, Nome = "Lilica", Jogador = true, Visivel = true, Coluna = 9, Linha = 9, Tamanho = 1 },
                new ParticipanteServiceModel { Id = 2, Nome = "Goblin à vista", Visivel = true, Coluna = 1, Linha = 1, Tamanho = 1 },
                new ParticipanteServiceModel { Id = 3, Nome = "Goblin escondido", Visivel = true, Coluna = 5, Linha = 5, Tamanho = 1 },
                new ParticipanteServiceModel { Id = 4, Nome = "Ogro meio revelado", Visivel = true, Coluna = 2, Linha = 2, Tamanho = 2 },
                new ParticipanteServiceModel { Id = 5, Nome = "Ogro revelado", Visivel = true, Coluna = 1, Linha = 1, Tamanho = 2 },
            ],
        };

        var visao = MesaService.FiltrarParaJogador(mesa, idRegistroJogador: null);

        Assert.Equal([1, 2, 5], visao.Participantes.Select(p => p.Id));
        Assert.True(visao.Nevoa);
        Assert.Equal(4, visao.CelulasReveladas.Count);
    }

    [Fact]
    public void FiltrarParaJogador_SemNevoa_NaoMandaCelulas()
    {
        var mesa = new MesaServiceModel { Nevoa = false, CelulasReveladas = [new() { Coluna = 1, Linha = 1 }] };
        Assert.Empty(MesaService.FiltrarParaJogador(mesa, null).CelulasReveladas);
    }

    [Fact]
    public async Task AlterarNevoaAsync_RevelaEEscondeEmLote_EAvisa()
    {
        List<(int, int)>? reveladas = null;
        List<(int, int)>? escondidas = null;
        _celulas.Setup(c => c.RevelarAsync(5, It.IsAny<IEnumerable<(int Coluna, int Linha)>>()))
            .Callback<int, IEnumerable<(int Coluna, int Linha)>>((_, l) => reveladas = l.Select(x => (x.Coluna, x.Linha)).ToList());
        _celulas.Setup(c => c.EsconderAsync(5, It.IsAny<IEnumerable<(int Coluna, int Linha)>>()))
            .Callback<int, IEnumerable<(int Coluna, int Linha)>>((_, l) => escondidas = l.Select(x => (x.Coluna, x.Linha)).ToList());
        _celulas.Setup(c => c.ListarPorMesaAsync(5)).ReturnsAsync([]);

        await CriarService().AlterarNevoaAsync(1, 5,
            [new() { Coluna = 1, Linha = 1 }, new() { Coluna = 1, Linha = 1 }, new() { Coluna = 2, Linha = 1 }],
            [new() { Coluna = 2, Linha = 1 }, new() { Coluna = 7, Linha = 7 }]);

        Assert.Equal([(1, 1), (2, 1)], reveladas!);
        Assert.Equal([(7, 7)], escondidas!);
        _canal.Verify(c => c.Avisar(10), Times.Once);
    }

    [Fact]
    public async Task AlterarNevoaAsync_MesaAlheia_Codigo17()
    {
        Assert.Equal(BusinessErrors.MesaNaoEncontrada.Code,
            await CodigoAsync(() => CriarService().AlterarNevoaAsync(1, 6, [new() { Coluna = 1, Linha = 1 }], [])));
        _celulas.Verify(c => c.RevelarAsync(It.IsAny<int>(), It.IsAny<IEnumerable<(int Coluna, int Linha)>>()), Times.Never);
    }
}
