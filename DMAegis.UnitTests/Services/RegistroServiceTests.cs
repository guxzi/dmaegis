using DMAegis.Model;
using DMAegis.Repository;
using DMAegis.Services;
using DMAegis.Services.Exceptions;
using Moq;

namespace DMAegis.UnitTests.Services;

public class RegistroServiceTests
{
    private readonly Mock<IRegistroRepository> _registros = new();
    private readonly Mock<IAtributoRegistroRepository> _atributos = new();
    private readonly Mock<ITagRepository> _tags = new();
    private readonly Mock<IRegistroTagRepository> _registroTags = new();
    private readonly Mock<ITipoRegistroRepository> _tipos = new();
    private readonly Mock<IModeloAtributoRepository> _modelos = new();
    private readonly Mock<ICampanhaService> _campanhas = new();

    private static readonly DateTime Ontem = DateTime.Now.AddDays(-1);

    public RegistroServiceTests()
    {
        _tipos.Setup(t => t.ListarAsync()).ReturnsAsync(Enumerable.Range(1, 7)
            .Select(i => new TipoRegistro { Id = i, Nome = $"Tipo {i}" }).ToList());

        _campanhas.Setup(c => c.ObterDoUsuarioOuLancarAsync(1, 10))
            .ReturnsAsync(new Campanha { Id = 10, IdUsuario = 1, IdSistema = 2 });
        _campanhas.Setup(c => c.ObterDoUsuarioOuLancarAsync(1, 20))
            .ThrowsAsync(new BusinessRuleException(BusinessErrors.CampanhaNaoEncontrada));

        _registros.Setup(r => r.ObterAsync(100)).ReturnsAsync(new Registro
        {
            Id = 100, IdCampanha = 10, IdTipoRegistro = 1, IdOrigemRegistro = 1, Nome = "Ulm",
            DataCriacao = Ontem, DataAtualizacao = Ontem,
        });
        _registros.Setup(r => r.ObterAsync(200)).ReturnsAsync(new Registro
        {
            Id = 200, IdCampanha = 20, IdTipoRegistro = 1, IdOrigemRegistro = 1, Nome = "Alheio",
        });
        _registros.Setup(r => r.ObterAsync(999)).ReturnsAsync((Registro?)null);

        _atributos.Setup(a => a.ListarPorRegistroAsync(It.IsAny<int>())).ReturnsAsync([]);
        _registroTags.Setup(r => r.ListarPorRegistrosAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync([]);
        _tags.Setup(t => t.ListarPorNomesAsync(It.IsAny<int>(), It.IsAny<IEnumerable<string>>())).ReturnsAsync([]);
    }

    private RegistroService CriarService() => new(_registros.Object, _atributos.Object, _tags.Object,
        _registroTags.Object, _tipos.Object, _modelos.Object, _campanhas.Object);

    private static SalvarRegistroServiceModel Dados(params string[] tags) => new()
    {
        IdTipoRegistro = 3,
        Nome = "  Lobo-de-cinzas ",
        Resumo = " Lobo cinzento ",
        Atributos = [new() { Chave = "Dificuldade", Valor = "12" }, new() { Chave = "PV", Valor = " " }],
        Tags = [.. tags],
    };

    [Fact]
    public void NormalizarTags_RemoveVaziasEspacosERepetidasSemAcentoEMaiuscula()
    {
        var tags = RegistroService.NormalizarTags(["Vila", " vila ", "víla", "", "  ", "Culto"]);
        Assert.Equal(["Vila", "Culto"], tags);
    }

    [Fact]
    public void NormalizarAtributos_DescartaValorVazioENumeraEmOrdem()
    {
        var atributos = RegistroService.NormalizarAtributos(
        [
            new() { Chave = " CA ", Valor = " 15 " },
            new() { Chave = "PV", Valor = "  " },
            new() { Chave = "Notas", Valor = "x" },
        ]);

        Assert.Collection(atributos,
            a => { Assert.Equal("CA", a.Chave); Assert.Equal("15", a.Valor); Assert.Equal(1, a.Ordem); },
            a => { Assert.Equal("Notas", a.Chave); Assert.Equal("x", a.Valor); Assert.Equal(2, a.Ordem); });
    }

    [Fact]
    public async Task InserirAsync_CampanhaAlheia_LancaCampanhaNaoEncontradaENaoGrava()
    {
        var erro = await Assert.ThrowsAsync<BusinessRuleException>(() => CriarService().InserirAsync(1, 20, Dados()));

        Assert.Equal(BusinessErrors.CampanhaNaoEncontrada.Code, erro.Code);
        _registros.Verify(r => r.InserirAsync(It.IsAny<Registro>()), Times.Never);
    }

    [Fact]
    public async Task InserirAsync_TipoInvalido_LancaCodigo8()
    {
        var dados = Dados();
        dados.IdTipoRegistro = 9;

        var erro = await Assert.ThrowsAsync<BusinessRuleException>(() => CriarService().InserirAsync(1, 10, dados));

        Assert.Equal(BusinessErrors.TipoRegistroInvalido.Code, erro.Code);
        _registros.Verify(r => r.InserirAsync(It.IsAny<Registro>()), Times.Never);
    }

    [Fact]
    public async Task InserirAsync_CriaSoAsTagsQueNaoExistemEVinculaTodas()
    {
        _registros.Setup(r => r.InserirAsync(It.IsAny<Registro>())).ReturnsAsync(300);
        _tags.Setup(t => t.ListarPorNomesAsync(10, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([new Tag { Id = 5, IdCampanha = 10, Nome = "vila" }]);
        _tags.Setup(t => t.InserirAsync(It.Is<Tag>(x => x.Nome == "culto" && x.IdCampanha == 10))).ReturnsAsync(6);
        List<int>? vinculadas = null;
        _registroTags.Setup(r => r.SubstituirAsync(300, It.IsAny<IEnumerable<int>>()))
            .Callback<int, IEnumerable<int>>((_, ids) => vinculadas = ids.ToList());

        await CriarService().InserirAsync(1, 10, Dados("Vila", "culto"));

        _tags.Verify(t => t.InserirAsync(It.IsAny<Tag>()), Times.Once);
        Assert.Equal([5, 6], vinculadas);
    }

    [Fact]
    public async Task InserirAsync_GravaRegistroComOrigemCriadoEAtributosNormalizados()
    {
        Registro? gravado = null;
        _registros.Setup(r => r.InserirAsync(It.IsAny<Registro>()))
            .Callback<Registro>(r => gravado = r).ReturnsAsync(300);
        List<AtributoRegistro>? atributos = null;
        _atributos.Setup(a => a.SubstituirAsync(300, It.IsAny<IEnumerable<AtributoRegistro>>()))
            .Callback<int, IEnumerable<AtributoRegistro>>((_, lista) => atributos = lista.ToList());

        var resultado = await CriarService().InserirAsync(1, 10, Dados());

        Assert.Equal("Lobo-de-cinzas", gravado!.Nome);
        Assert.Equal("Lobo cinzento", gravado.Resumo);
        Assert.Equal(10, gravado.IdCampanha);
        Assert.Equal(1, gravado.IdOrigemRegistro);
        var atributo = Assert.Single(atributos!);
        Assert.Equal(("Dificuldade", "12", 1), (atributo.Chave, atributo.Valor, atributo.Ordem));
        Assert.Equal(300, resultado.Id);
    }

    [Fact]
    public async Task ObterAsync_RegistroDeCampanhaAlheia_LancaCodigo7()
    {
        var erro = await Assert.ThrowsAsync<BusinessRuleException>(() => CriarService().ObterAsync(1, 200));
        Assert.Equal(BusinessErrors.RegistroNaoEncontrado.Code, erro.Code);
    }

    [Fact]
    public async Task ObterAsync_RegistroInexistente_LancaCodigo7()
    {
        var erro = await Assert.ThrowsAsync<BusinessRuleException>(() => CriarService().ObterAsync(1, 999));
        Assert.Equal(BusinessErrors.RegistroNaoEncontrado.Code, erro.Code);
        Assert.Equal("Registro não encontrado.", erro.Mensagem);
    }

    [Fact]
    public async Task AtualizarAsync_MantemCampanhaEAtualizaData()
    {
        Registro? gravado = null;
        _registros.Setup(r => r.AtualizarAsync(It.IsAny<Registro>())).Callback<Registro>(r => gravado = r);

        await CriarService().AtualizarAsync(1, 100, Dados("vila"));

        Assert.Equal(10, gravado!.IdCampanha);
        Assert.Equal(3, gravado.IdTipoRegistro);
        Assert.True(gravado.DataAtualizacao > gravado.DataCriacao);
        _atributos.Verify(a => a.SubstituirAsync(100, It.IsAny<IEnumerable<AtributoRegistro>>()), Times.Once);
        _registroTags.Verify(r => r.SubstituirAsync(100, It.IsAny<IEnumerable<int>>()), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_RegistroAlheio_NaoGrava()
    {
        var erro = await Assert.ThrowsAsync<BusinessRuleException>(() => CriarService().AtualizarAsync(1, 200, Dados()));
        Assert.Equal(BusinessErrors.RegistroNaoEncontrado.Code, erro.Code);
        _registros.Verify(r => r.AtualizarAsync(It.IsAny<Registro>()), Times.Never);
    }

    [Fact]
    public async Task ExcluirAsync_RegistroAlheio_NaoExclui()
    {
        var erro = await Assert.ThrowsAsync<BusinessRuleException>(() => CriarService().ExcluirAsync(1, 200));
        Assert.Equal(BusinessErrors.RegistroNaoEncontrado.Code, erro.Code);
        _registros.Verify(r => r.ExcluirAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ListarAsync_RepassaFiltrosComTextoAparado()
    {
        FiltroRegistro? recebido = null;
        _registros.Setup(r => r.ListarAsync(10, It.IsAny<FiltroRegistro>()))
            .Callback<int, FiltroRegistro>((_, f) => recebido = f)
            .ReturnsAsync([new Registro { Id = 100, IdCampanha = 10, IdTipoRegistro = 1, Nome = "Ulm" }]);
        _registroTags.Setup(r => r.ListarPorRegistrosAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync([(100, new Tag { Id = 5, IdCampanha = 10, Nome = "vila" })]);

        var lista = (await CriarService().ListarAsync(1, 10,
            new FiltroRegistro { IdTipoRegistro = 1, IdTag = 5, Texto = "  ulm " })).ToList();

        Assert.Equal("ulm", recebido!.Texto);
        Assert.Equal(1, recebido.IdTipoRegistro);
        Assert.Equal(5, recebido.IdTag);
        Assert.Equal("vila", Assert.Single(Assert.Single(lista).Tags).Nome);

        await CriarService().ListarAsync(1, 10, new FiltroRegistro { Texto = "   " });
        Assert.Null(recebido.Texto);
    }

    [Fact]
    public async Task BuscarAsync_TermoVazio_NaoConsultaEDevolveVazio()
    {
        var lista = await CriarService().BuscarAsync(1, 10, "   ");

        Assert.Empty(lista);
        _registros.Verify(r => r.BuscarAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task BuscarAsync_AparaTermoELimita10ETrazTags()
    {
        _registros.Setup(r => r.BuscarAsync(10, "ulm", 10))
            .ReturnsAsync([new Registro { Id = 100, IdCampanha = 10, IdTipoRegistro = 1, Nome = "Ulm" }]);
        _registroTags.Setup(r => r.ListarPorRegistrosAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync([(100, new Tag { Id = 5, IdCampanha = 10, Nome = "vila" })]);

        var lista = (await CriarService().BuscarAsync(1, 10, "  ulm ")).ToList();

        Assert.Equal("Ulm", Assert.Single(lista).Nome);
        Assert.Equal("vila", Assert.Single(lista[0].Tags).Nome);
        _registros.Verify(r => r.BuscarAsync(10, "ulm", 10), Times.Once);
    }

    [Fact]
    public async Task BuscarAsync_CampanhaAlheia_Codigo5()
    {
        var erro = await Assert.ThrowsAsync<BusinessRuleException>(() => CriarService().BuscarAsync(1, 20, "ulm"));
        Assert.Equal(BusinessErrors.CampanhaNaoEncontrada.Code, erro.Code);
    }
}
