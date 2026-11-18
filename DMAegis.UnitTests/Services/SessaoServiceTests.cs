using DMAegis.Model;
using DMAegis.Repository;
using DMAegis.Services;
using DMAegis.Services.Exceptions;
using Moq;

namespace DMAegis.UnitTests.Services;

public class SessaoServiceTests
{
    private readonly Mock<ISessaoRepository> _sessoes = new();
    private readonly Mock<ICenaSessaoRepository> _cenas = new();
    private readonly Mock<ISessaoRegistroRepository> _vinculos = new();
    private readonly Mock<ISegredoRepository> _segredos = new();
    private readonly Mock<IRegistroRepository> _registros = new();
    private readonly Mock<ICampanhaService> _campanhas = new();
    private readonly Mock<IAnotacaoSessaoRepository> _anotacoes = new();

    public SessaoServiceTests()
    {
        _campanhas.Setup(c => c.ObterDoUsuarioOuLancarAsync(1, 10))
            .ReturnsAsync(new Campanha { Id = 10, IdUsuario = 1, IdSistema = 1 });
        _campanhas.Setup(c => c.ObterDoUsuarioOuLancarAsync(1, 20))
            .ThrowsAsync(new BusinessRuleException(BusinessErrors.CampanhaNaoEncontrada));

        _sessoes.Setup(s => s.ObterAsync(1)).ReturnsAsync(new Sessao { Id = 1, IdCampanha = 10, Numero = 1 });
        _sessoes.Setup(s => s.ObterAsync(2)).ReturnsAsync(new Sessao { Id = 2, IdCampanha = 10, Numero = 2 });
        _sessoes.Setup(s => s.ObterAsync(9)).ReturnsAsync(new Sessao { Id = 9, IdCampanha = 20, Numero = 1 });
        _sessoes.Setup(s => s.ObterAsync(99)).ReturnsAsync((Sessao?)null);

        _registros.Setup(r => r.ListarAsync(10, It.IsAny<FiltroRegistro>())).ReturnsAsync(
        [
            new Registro { Id = 100, IdCampanha = 10, Nome = "Ulm" },
            new Registro { Id = 101, IdCampanha = 10, Nome = "Mina" },
        ]);

        _segredos.Setup(s => s.ObterAsync(50)).ReturnsAsync(new Segredo { Id = 50, IdCampanha = 10, IdSessaoCriacao = 1, Texto = "Cultista" });
        _segredos.Setup(s => s.ObterAsync(60)).ReturnsAsync(new Segredo { Id = 60, IdCampanha = 20, Texto = "Alheio" });

        _cenas.Setup(c => c.ListarPorSessaoAsync(It.IsAny<int>())).ReturnsAsync([]);
        _vinculos.Setup(v => v.ListarIdsRegistroAsync(It.IsAny<int>())).ReturnsAsync([]);
        _segredos.Setup(s => s.ListarDaSessaoAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync([]);
    }

    private SessaoService CriarService() => new(_sessoes.Object, _cenas.Object, _vinculos.Object,
        _segredos.Object, _registros.Object, _campanhas.Object, _anotacoes.Object);

    private static SalvarSessaoServiceModel Preparo(params int[] idsRegistro) => new()
    {
        Numero = 2,
        ComecoForte = "Emboscada",
        Cenas = [" Ponte ", "", "Vila"],
        IdsRegistro = [.. idsRegistro],
    };

    private static async Task<int> CodigoAsync(Func<Task> acao) =>
        (await Assert.ThrowsAsync<BusinessRuleException>(acao)).Code;

    [Fact]
    public async Task InserirAsync_SemNumero_UsaOMaiorMaisUm()
    {
        _sessoes.Setup(s => s.ObterMaiorNumeroAsync(10)).ReturnsAsync(2);
        Sessao? gravada = null;
        _sessoes.Setup(s => s.InserirAsync(It.IsAny<Sessao>())).Callback<Sessao>(s => gravada = s).ReturnsAsync(3);
        _sessoes.Setup(s => s.ObterAsync(3)).ReturnsAsync(() => gravada);

        var resultado = await CriarService().InserirAsync(1, 10, new InserirSessaoServiceModel());

        Assert.Equal(3, gravada!.Numero);
        Assert.Equal(10, gravada.IdCampanha);
        Assert.Equal(3, resultado.Numero);
    }

    [Fact]
    public async Task InserirAsync_NumeroDuplicado_Lanca409Codigo9()
    {
        _sessoes.Setup(s => s.InserirAsync(It.IsAny<Sessao>())).ThrowsAsync(new RegistroDuplicadoException());

        Assert.Equal(BusinessErrors.SessaoNumeroDuplicado.Code,
            await CodigoAsync(() => CriarService().InserirAsync(1, 10, new InserirSessaoServiceModel { Numero = 1 })));
    }

    [Fact]
    public async Task InserirAsync_CampanhaAlheia_LancaCodigo5()
    {
        Assert.Equal(BusinessErrors.CampanhaNaoEncontrada.Code,
            await CodigoAsync(() => CriarService().InserirAsync(1, 20, new InserirSessaoServiceModel())));
        _sessoes.Verify(s => s.InserirAsync(It.IsAny<Sessao>()), Times.Never);
    }

    [Fact]
    public async Task ObterAsync_SessaoAlheia_LancaCodigo10()
    {
        Assert.Equal(BusinessErrors.SessaoNaoEncontrada.Code, await CodigoAsync(() => CriarService().ObterAsync(1, 9)));
        Assert.Equal(BusinessErrors.SessaoNaoEncontrada.Code, await CodigoAsync(() => CriarService().ObterAsync(1, 99)));
    }

    [Fact]
    public async Task ObterAsync_TrazCenasVinculosESegredosDaSessao()
    {
        _cenas.Setup(c => c.ListarPorSessaoAsync(2)).ReturnsAsync(
        [
            new CenaSessao { IdSessao = 2, Descricao = "Vila", Ordem = 2 },
            new CenaSessao { IdSessao = 2, Descricao = "Ponte", Ordem = 1 },
        ]);
        _vinculos.Setup(v => v.ListarIdsRegistroAsync(2)).ReturnsAsync([100]);
        _segredos.Setup(s => s.ListarDaSessaoAsync(10, 2)).ReturnsAsync([new Segredo { Id = 50, IdCampanha = 10, Texto = "Cultista" }]);

        var sessao = await CriarService().ObterAsync(1, 2);

        Assert.Equal(["Ponte", "Vila"], sessao.Cenas);
        Assert.Equal([100], sessao.IdsRegistro);
        Assert.Equal("Cultista", Assert.Single(sessao.Segredos).Texto);
        _segredos.Verify(s => s.ListarDaSessaoAsync(10, 2), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_DescartaCenasVaziasENumeraEmOrdem()
    {
        List<CenaSessao>? gravadas = null;
        _cenas.Setup(c => c.SubstituirAsync(2, It.IsAny<IEnumerable<CenaSessao>>()))
            .Callback<int, IEnumerable<CenaSessao>>((_, lista) => gravadas = lista.ToList());

        await CriarService().AtualizarAsync(1, 2, Preparo());

        Assert.Equal([("Ponte", 1), ("Vila", 2)], gravadas!.Select(c => (c.Descricao, c.Ordem)));
    }

    [Fact]
    public async Task AtualizarAsync_VinculoDeOutraCampanha_LancaCodigo7ENaoGrava()
    {
        Assert.Equal(BusinessErrors.RegistroNaoEncontrado.Code,
            await CodigoAsync(() => CriarService().AtualizarAsync(1, 2, Preparo(100, 200))));
        _sessoes.Verify(s => s.AtualizarAsync(It.IsAny<Sessao>()), Times.Never);
        _vinculos.Verify(v => v.SubstituirAsync(It.IsAny<int>(), It.IsAny<IEnumerable<int>>()), Times.Never);
    }

    [Fact]
    public async Task AtualizarAsync_VinculosRepetidosSaoIgnorados()
    {
        List<int>? gravados = null;
        _vinculos.Setup(v => v.SubstituirAsync(2, It.IsAny<IEnumerable<int>>()))
            .Callback<int, IEnumerable<int>>((_, ids) => gravados = ids.ToList());

        await CriarService().AtualizarAsync(1, 2, Preparo(100, 100, 101));

        Assert.Equal([100, 101], gravados);
    }

    [Fact]
    public async Task AtualizarAsync_NumeroDuplicado_Lanca409()
    {
        _sessoes.Setup(s => s.AtualizarAsync(It.IsAny<Sessao>())).ThrowsAsync(new RegistroDuplicadoException());

        Assert.Equal(BusinessErrors.SessaoNumeroDuplicado.Code,
            await CodigoAsync(() => CriarService().AtualizarAsync(1, 2, Preparo())));
    }

    [Fact]
    public async Task InserirSegredoAsync_PertenceACampanhaECriadoNaSessao()
    {
        Segredo? gravado = null;
        _segredos.Setup(s => s.InserirAsync(It.IsAny<Segredo>())).Callback<Segredo>(s => gravado = s).ReturnsAsync(70);

        var segredo = await CriarService().InserirSegredoAsync(1, 2, "  A ponte está podre  ");

        Assert.Equal(10, gravado!.IdCampanha);
        Assert.Equal(2, gravado.IdSessaoCriacao);
        Assert.Null(gravado.IdSessaoRevelacao);
        Assert.Equal("A ponte está podre", gravado.Texto);
        Assert.Equal(70, segredo.Id);
    }

    [Fact]
    public async Task RevelarSegredoAsync_GravaSessaoEData()
    {
        Segredo? gravado = null;
        _segredos.Setup(s => s.AtualizarAsync(It.IsAny<Segredo>())).Callback<Segredo>(s => gravado = s);

        var segredo = await CriarService().RevelarSegredoAsync(1, 50, 2);

        Assert.Equal(2, gravado!.IdSessaoRevelacao);
        Assert.NotNull(gravado.DataRevelacao);
        Assert.Equal(2, segredo.IdSessaoRevelacao);
    }

    [Fact]
    public async Task RevelarSegredoAsync_SessaoDeOutraCampanha_LancaCodigo10()
    {
        Assert.Equal(BusinessErrors.SessaoNaoEncontrada.Code,
            await CodigoAsync(() => CriarService().RevelarSegredoAsync(1, 50, 9)));
        _segredos.Verify(s => s.AtualizarAsync(It.IsAny<Segredo>()), Times.Never);
    }

    [Fact]
    public async Task OcultarSegredoAsync_LimpaSessaoEData()
    {
        _segredos.Setup(s => s.ObterAsync(50)).ReturnsAsync(new Segredo
        {
            Id = 50, IdCampanha = 10, Texto = "Cultista", IdSessaoRevelacao = 1, DataRevelacao = DateTime.Now,
        });
        Segredo? gravado = null;
        _segredos.Setup(s => s.AtualizarAsync(It.IsAny<Segredo>())).Callback<Segredo>(s => gravado = s);

        await CriarService().OcultarSegredoAsync(1, 50);

        Assert.Null(gravado!.IdSessaoRevelacao);
        Assert.Null(gravado.DataRevelacao);
    }

    [Fact]
    public async Task SegredoAlheio_LancaCodigo11()
    {
        var service = CriarService();
        var codigo = BusinessErrors.SegredoNaoEncontrado.Code;

        Assert.Equal(codigo, await CodigoAsync(() => service.AtualizarSegredoAsync(1, 60, "x")));
        Assert.Equal(codigo, await CodigoAsync(() => service.RevelarSegredoAsync(1, 60, 1)));
        Assert.Equal(codigo, await CodigoAsync(() => service.OcultarSegredoAsync(1, 60)));
        Assert.Equal(codigo, await CodigoAsync(() => service.ExcluirSegredoAsync(1, 60)));
        _segredos.Verify(s => s.AtualizarAsync(It.IsAny<Segredo>()), Times.Never);
        _segredos.Verify(s => s.ExcluirAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task InserirAnotacaoAsync_AparaTextoEUsaHoraDoServidor()
    {
        AnotacaoSessao? gravada = null;
        _anotacoes.Setup(a => a.InserirAsync(It.IsAny<AnotacaoSessao>()))
            .Callback<AnotacaoSessao>(a => gravada = a).ReturnsAsync(40);
        var antes = DateTime.Now;

        var anotacao = await CriarService().InserirAnotacaoAsync(1, 2, "  Lia pagou 30 po  ");

        Assert.Equal("Lia pagou 30 po", gravada!.Texto);
        Assert.Equal(2, gravada.IdSessao);
        Assert.InRange(gravada.Data, antes.AddSeconds(-1), DateTime.Now.AddSeconds(1));
        Assert.Equal(40, anotacao.Id);
    }

    [Fact]
    public async Task InserirAnotacaoAsync_SessaoAlheia_Codigo10()
    {
        Assert.Equal(BusinessErrors.SessaoNaoEncontrada.Code,
            await CodigoAsync(() => CriarService().InserirAnotacaoAsync(1, 9, "x")));
        _anotacoes.Verify(a => a.InserirAsync(It.IsAny<AnotacaoSessao>()), Times.Never);
    }

    [Fact]
    public async Task ExcluirAnotacaoAsync_DeSessaoAlheiaOuInexistente_Codigo12()
    {
        _anotacoes.Setup(a => a.ObterAsync(41)).ReturnsAsync(new AnotacaoSessao { Id = 41, IdSessao = 9, Texto = "x" });
        _anotacoes.Setup(a => a.ObterAsync(42)).ReturnsAsync((AnotacaoSessao?)null);

        Assert.Equal(BusinessErrors.AnotacaoNaoEncontrada.Code, await CodigoAsync(() => CriarService().ExcluirAnotacaoAsync(1, 41)));
        Assert.Equal(BusinessErrors.AnotacaoNaoEncontrada.Code, await CodigoAsync(() => CriarService().ExcluirAnotacaoAsync(1, 42)));
        _anotacoes.Verify(a => a.ExcluirAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ListarAnotacoesAsync_DevolveNaOrdemDoRepositorio()
    {
        _anotacoes.Setup(a => a.ListarPorSessaoAsync(2)).ReturnsAsync(
        [
            new AnotacaoSessao { Id = 1, IdSessao = 2, Data = new DateTime(2026, 11, 18, 20, 14, 0), Texto = "primeira" },
            new AnotacaoSessao { Id = 2, IdSessao = 2, Data = new DateTime(2026, 11, 18, 20, 31, 0), Texto = "segunda" },
        ]);

        var lista = (await CriarService().ListarAnotacoesAsync(1, 2)).ToList();

        Assert.Equal(["primeira", "segunda"], lista.Select(a => a.Texto));
    }
}
