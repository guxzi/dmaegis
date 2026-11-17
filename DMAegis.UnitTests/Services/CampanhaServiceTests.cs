using DMAegis.Model;
using DMAegis.Repository;
using DMAegis.Services;
using DMAegis.Services.Exceptions;
using Moq;

namespace DMAegis.UnitTests.Services;

public class CampanhaServiceTests
{
    private readonly Mock<ICampanhaRepository> _campanhas = new();
    private readonly Mock<ISistemaRepository> _sistemas = new();

    private static Campanha DoUsuario1() => new()
    {
        Id = 10, IdUsuario = 1, IdSistema = 2, Nome = "Sementes", Descricao = "Floresta", DataCriacao = DateTime.Today,
    };

    private static Campanha DoUsuario2() => new()
    {
        Id = 20, IdUsuario = 2, IdSistema = 1, Nome = "Alheia", DataCriacao = DateTime.Today,
    };

    public CampanhaServiceTests()
    {
        _sistemas.Setup(s => s.ListarAsync()).ReturnsAsync(
        [
            new Sistema { Id = 1, Nome = "D&D 5e (2024)" },
            new Sistema { Id = 2, Nome = "Daggerheart" },
        ]);
        _campanhas.Setup(c => c.ObterAsync(10)).ReturnsAsync(DoUsuario1);
        _campanhas.Setup(c => c.ObterAsync(20)).ReturnsAsync(DoUsuario2);
        _campanhas.Setup(c => c.ObterAsync(99)).ReturnsAsync((Campanha?)null);
    }

    private CampanhaService CriarService() => new(_campanhas.Object, _sistemas.Object);

    [Fact]
    public async Task ListarAsync_DevolveSoAsDoUsuarioComNomeDoSistema()
    {
        _campanhas.Setup(c => c.ListarPorUsuarioAsync(1)).ReturnsAsync([DoUsuario1()]);

        var lista = (await CriarService().ListarAsync(1)).ToList();

        var campanha = Assert.Single(lista);
        Assert.Equal(10, campanha.Id);
        Assert.Equal("Daggerheart", campanha.NomeSistema);
        _campanhas.Verify(c => c.ListarPorUsuarioAsync(1), Times.Once);
    }

    [Fact]
    public async Task ObterAsync_CampanhaDeOutroUsuario_LancaNaoEncontrada()
    {
        var erro = await Assert.ThrowsAsync<BusinessRuleException>(() => CriarService().ObterAsync(1, 20));
        Assert.Equal(BusinessErrors.CampanhaNaoEncontrada.Code, erro.Code);
    }

    [Fact]
    public async Task ObterAsync_CampanhaInexistente_LancaNaoEncontrada()
    {
        var erro = await Assert.ThrowsAsync<BusinessRuleException>(() => CriarService().ObterAsync(1, 99));
        Assert.Equal(BusinessErrors.CampanhaNaoEncontrada.Code, erro.Code);
        Assert.Equal("Campanha não encontrada.", erro.Mensagem);
    }

    [Fact]
    public async Task InserirAsync_GravaComUsuarioLogadoENomeSemEspacos()
    {
        Campanha? gravada = null;
        _campanhas.Setup(c => c.InserirAsync(It.IsAny<Campanha>()))
            .Callback<Campanha>(c => gravada = c).ReturnsAsync(30);

        var resultado = await CriarService().InserirAsync(1, new InserirCampanhaServiceModel
        {
            IdSistema = 1, Nome = "  Mina  ", Descricao = "   ",
        });

        Assert.Equal("Mina", gravada!.Nome);
        Assert.Equal(1, gravada.IdUsuario);
        Assert.False(gravada.Arquivada);
        Assert.Null(gravada.Descricao);
        Assert.Equal(30, resultado.Id);
        Assert.Equal("D&D 5e (2024)", resultado.NomeSistema);
    }

    [Fact]
    public async Task InserirAsync_SistemaInexistente_LancaSistemaInvalido()
    {
        var erro = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CriarService().InserirAsync(1, new InserirCampanhaServiceModel { IdSistema = 9, Nome = "X" }));

        Assert.Equal(BusinessErrors.SistemaInvalido.Code, erro.Code);
        _campanhas.Verify(c => c.InserirAsync(It.IsAny<Campanha>()), Times.Never);
    }

    [Fact]
    public async Task AtualizarAsync_MantemSistemaEUsuario()
    {
        Campanha? gravada = null;
        _campanhas.Setup(c => c.AtualizarAsync(It.IsAny<Campanha>())).Callback<Campanha>(c => gravada = c);

        var resultado = await CriarService().AtualizarAsync(1, 10,
            new AtualizarCampanhaServiceModel { Nome = "Sementes da Ruína", Descricao = "Nova" });

        Assert.Equal("Sementes da Ruína", gravada!.Nome);
        Assert.Equal("Nova", gravada.Descricao);
        Assert.Equal(2, gravada.IdSistema);
        Assert.Equal(1, gravada.IdUsuario);
        Assert.Equal("Daggerheart", resultado.NomeSistema);
    }

    [Fact]
    public async Task AtualizarAsync_CampanhaDeOutroUsuario_NaoGrava()
    {
        var erro = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CriarService().AtualizarAsync(1, 20, new AtualizarCampanhaServiceModel { Nome = "Roubada" }));

        Assert.Equal(BusinessErrors.CampanhaNaoEncontrada.Code, erro.Code);
        _campanhas.Verify(c => c.AtualizarAsync(It.IsAny<Campanha>()), Times.Never);
    }

    [Fact]
    public async Task DefinirArquivadaAsync_ArquivaEDesarquiva()
    {
        var gravadas = new List<bool>();
        _campanhas.Setup(c => c.AtualizarAsync(It.IsAny<Campanha>())).Callback<Campanha>(c => gravadas.Add(c.Arquivada));

        var arquivada = await CriarService().DefinirArquivadaAsync(1, 10, true);
        var desarquivada = await CriarService().DefinirArquivadaAsync(1, 10, false);

        Assert.True(arquivada.Arquivada);
        Assert.False(desarquivada.Arquivada);
        Assert.Equal([true, false], gravadas);
    }

    [Fact]
    public async Task ExcluirAsync_CampanhaDeOutroUsuario_NaoExclui()
    {
        var erro = await Assert.ThrowsAsync<BusinessRuleException>(() => CriarService().ExcluirAsync(1, 20));

        Assert.Equal(BusinessErrors.CampanhaNaoEncontrada.Code, erro.Code);
        _campanhas.Verify(c => c.ExcluirAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ExcluirAsync_CampanhaPropria_Exclui()
    {
        await CriarService().ExcluirAsync(1, 10);
        _campanhas.Verify(c => c.ExcluirAsync(10), Times.Once);
    }
}
