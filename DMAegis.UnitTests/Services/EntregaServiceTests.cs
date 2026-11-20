using DMAegis.Model;
using DMAegis.Repository;
using DMAegis.Services;
using DMAegis.Services.Exceptions;
using Moq;

namespace DMAegis.UnitTests.Services;

public class EntregaServiceTests
{
    private readonly Mock<IEntregaRepository> _entregas = new();
    private readonly Mock<IImagemRegistroRepository> _imagens = new();
    private readonly Mock<IRegistroRepository> _registros = new();
    private readonly Mock<ICampanhaService> _campanhas = new();
    private readonly Mock<IJogadorService> _jogadores = new();
    private readonly Mock<ICanalMesa> _canal = new();

    public EntregaServiceTests()
    {
        _campanhas.Setup(c => c.ObterDoUsuarioOuLancarAsync(1, 10)).ReturnsAsync(new Campanha { Id = 10, IdUsuario = 1 });
        _campanhas.Setup(c => c.ObterDoUsuarioOuLancarAsync(1, 20)).ThrowsAsync(new BusinessRuleException(BusinessErrors.CampanhaNaoEncontrada));
        _imagens.Setup(i => i.ListarPorCampanhaAsync(10)).ReturnsAsync([new ImagemDaCampanha { Id = 300, IdRegistro = 100 }]);
        _registros.Setup(r => r.ObterAsync(100)).ReturnsAsync(new Registro { Id = 100, IdCampanha = 10, IdTipoRegistro = 2, Nome = "Lilica Lovejoy" });
        _registros.Setup(r => r.ObterAsync(101)).ReturnsAsync(new Registro { Id = 101, IdCampanha = 10, IdTipoRegistro = 3, Nome = "Goblin" });
        _registros.Setup(r => r.ObterAsync(900)).ReturnsAsync(new Registro { Id = 900, IdCampanha = 20, IdTipoRegistro = 2, Nome = "Alheio" });
    }

    private EntregaService CriarService() => new(_entregas.Object, _imagens.Object, _registros.Object, _campanhas.Object, _jogadores.Object, _canal.Object);

    private static async Task<int> CodigoAsync(Func<Task> acao) =>
        (await Assert.ThrowsAsync<BusinessRuleException>(acao)).Code;

    [Fact]
    public async Task InserirAsync_SoParaUmPj_GuardaEAvisaAMesa()
    {
        Entrega? salva = null;
        _entregas.Setup(e => e.InserirAsync(It.IsAny<Entrega>())).Callback<Entrega>(e => salva = e).ReturnsAsync(7);

        var entrega = await CriarService().InserirAsync(1, 10, new InserirEntregaServiceModel
        {
            Titulo = " Carta do prefeito ", Texto = "  Venha sozinha.  ", IdImagemRegistro = 300, IdRegistroDestino = 100,
        });

        Assert.Equal(("Carta do prefeito", "Venha sozinha.", 100), (salva!.Titulo, salva.Texto, salva.IdRegistroDestino));
        Assert.Equal((7, "Lilica Lovejoy"), (entrega.Id, entrega.NomeDestino));
        _canal.Verify(c => c.Avisar(10), Times.Once);
    }

    [Fact]
    public async Task InserirAsync_RegrasDeImagemDestinoEConteudo()
    {
        Assert.Equal(BusinessErrors.DadosInvalidos.Code,
            await CodigoAsync(() => CriarService().InserirAsync(1, 10, new InserirEntregaServiceModel { Titulo = "Vazia", Texto = "  " })));
        Assert.Equal(BusinessErrors.ImagemNaoEncontrada.Code,
            await CodigoAsync(() => CriarService().InserirAsync(1, 10, new InserirEntregaServiceModel { Titulo = "X", IdImagemRegistro = 999 })));
        Assert.Equal(BusinessErrors.SoJogadorTemConvite.Code,
            await CodigoAsync(() => CriarService().InserirAsync(1, 10, new InserirEntregaServiceModel { Titulo = "X", Texto = "t", IdRegistroDestino = 101 })));
        Assert.Equal(BusinessErrors.RegistroNaoEncontrado.Code,
            await CodigoAsync(() => CriarService().InserirAsync(1, 10, new InserirEntregaServiceModel { Titulo = "X", Texto = "t", IdRegistroDestino = 900 })));
        Assert.Equal(BusinessErrors.CampanhaNaoEncontrada.Code,
            await CodigoAsync(() => CriarService().InserirAsync(1, 20, new InserirEntregaServiceModel { Titulo = "X", Texto = "t" })));
    }

    [Fact]
    public async Task ExcluirAsync_EntregaDeOutraCampanha_Codigo25()
    {
        _entregas.Setup(e => e.ObterAsync(8)).ReturnsAsync(new Entrega { Id = 8, IdCampanha = 20 });
        Assert.Equal(BusinessErrors.EntregaNaoEncontrada.Code, await CodigoAsync(() => CriarService().ExcluirAsync(1, 8)));
        Assert.Equal(BusinessErrors.EntregaNaoEncontrada.Code, await CodigoAsync(() => CriarService().ExcluirAsync(1, 99)));
        _entregas.Verify(e => e.ExcluirAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ListarDoJogadorAsync_ConfereOConvite_EUsaOFiltroDoJogador()
    {
        var lilica = new JogadorLogadoServiceModel { IdCampanha = 10, IdRegistro = 100 };
        _entregas.Setup(e => e.ListarDoJogadorAsync(10, 100)).ReturnsAsync([new Entrega { Id = 1, Titulo = "Para todos" }]);

        var lista = await CriarService().ListarDoJogadorAsync(lilica);

        Assert.Single(lista);
        _jogadores.Verify(j => j.ConferirAcessoAsync(lilica), Times.Once);
    }
}
