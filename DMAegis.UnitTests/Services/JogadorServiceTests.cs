using DMAegis.Model;
using DMAegis.Repository;
using DMAegis.Services;
using DMAegis.Services.Exceptions;
using Moq;

namespace DMAegis.UnitTests.Services;

public class JogadorServiceTests
{
    private readonly Mock<IConviteJogadorRepository> _convites = new();
    private readonly Mock<IRegistroRepository> _registros = new();
    private readonly Mock<ICampanhaRepository> _campanhasRepo = new();
    private readonly Mock<IParticipanteMesaRepository> _participantes = new();
    private readonly Mock<IImagemRegistroRepository> _imagens = new();
    private readonly Mock<IRegistroService> _registroService = new();
    private readonly Mock<ICampanhaService> _campanhas = new();
    private readonly Mock<IMesaService> _mesas = new();
    private readonly Mock<ICanalMesa> _canal = new();

    private const string Codigo = "codigo-de-teste-bem-comprido-0123456789abc";
    private static readonly string Hash = JogadorService.HashDoCodigo(Codigo);

    private readonly JogadorLogadoServiceModel _lilica = new() { IdCampanha = 10, IdRegistro = 100, Marca = Hash[..JogadorService.TamanhoMarca] };

    public JogadorServiceTests()
    {
        _convites.Setup(c => c.ObterPorHashAsync(Hash)).ReturnsAsync(new ConviteJogador { Id = 1, IdCampanha = 10, IdRegistro = 100, HashCodigo = Hash });
        _convites.Setup(c => c.ObterPorRegistroAsync(100)).ReturnsAsync(new ConviteJogador { Id = 1, IdCampanha = 10, IdRegistro = 100, HashCodigo = Hash });
        _registros.Setup(r => r.ObterAsync(100)).ReturnsAsync(new Registro { Id = 100, IdCampanha = 10, IdTipoRegistro = 2, Nome = "Lilica Lovejoy" });
        _campanhasRepo.Setup(c => c.ObterAsync(10)).ReturnsAsync(new Campanha { Id = 10, Nome = "Mesa de teste" });
    }

    private JogadorService CriarService() => new(_convites.Object, _registros.Object, _campanhasRepo.Object, _participantes.Object,
        _imagens.Object, _registroService.Object, _campanhas.Object, _mesas.Object, _canal.Object);

    private static async Task<int> CodigoAsync(Func<Task> acao) =>
        (await Assert.ThrowsAsync<BusinessRuleException>(acao)).Code;

    private void MesaAtiva(bool movimentoLivre = true) =>
        _mesas.Setup(m => m.MontarAtivaAsync(10)).ReturnsAsync(new MesaServiceModel
        {
            Id = 5, IdCampanha = 10, IdImagemRegistro = 300, MovimentoLivre = movimentoLivre,
            Participantes =
            [
                new ParticipanteServiceModel { Id = 1, IdRegistro = 100, Nome = "Lilica", Jogador = true, Visivel = true, Coluna = 2, Linha = 2, IdImagemRetrato = 301 },
                new ParticipanteServiceModel { Id = 2, IdRegistro = 101, Nome = "Goblin", Visivel = true, Coluna = 5, Linha = 5, PvAtual = 3, PvMaximo = 7 },
                new ParticipanteServiceModel { Id = 3, IdRegistro = 102, Nome = "Emboscada", Visivel = false, Coluna = 8, Linha = 1, IdImagemRetrato = 302 },
            ],
        });

    [Fact]
    public void NovoCodigo_ETrintaEDoisBytesSegurosParaUrl_EDiferenteACadaVez()
    {
        var a = JogadorService.NovoCodigo();
        Assert.Equal(43, a.Length);
        Assert.DoesNotContain('+', a);
        Assert.DoesNotContain('/', a);
        Assert.NotEqual(a, JogadorService.NovoCodigo());
        Assert.Equal(64, JogadorService.HashDoCodigo(a).Length);
    }

    [Fact]
    public async Task GerarConviteAsync_GuardaSoOHash_EAvisaAMesa()
    {
        _registroService.Setup(r => r.ObterAsync(1, 100)).ReturnsAsync(new RegistroServiceModel { Id = 100, IdCampanha = 10, IdTipoRegistro = 2 });
        ConviteJogador? salvo = null;
        _convites.Setup(c => c.SalvarAsync(It.IsAny<ConviteJogador>())).Callback<ConviteJogador>(c => salvo = c);

        var codigo = await CriarService().GerarConviteAsync(1, 100);

        Assert.Equal(JogadorService.HashDoCodigo(codigo), salvo!.HashCodigo);
        Assert.NotEqual(codigo, salvo.HashCodigo);
        _canal.Verify(c => c.Avisar(10), Times.Once);
    }

    [Fact]
    public async Task GerarConviteAsync_RegistroQueNaoEPj_Codigo24()
    {
        _registroService.Setup(r => r.ObterAsync(1, 101)).ReturnsAsync(new RegistroServiceModel { Id = 101, IdCampanha = 10, IdTipoRegistro = 3 });
        Assert.Equal(BusinessErrors.SoJogadorTemConvite.Code, await CodigoAsync(() => CriarService().GerarConviteAsync(1, 101)));
    }

    [Fact]
    public async Task EntrarAsync_CodigoCerto_DevolveOJogadorComAMarca()
    {
        var jogador = await CriarService().EntrarAsync(Codigo);

        Assert.Equal((10, 100, "Lilica Lovejoy", "Mesa de teste"), (jogador.IdCampanha, jogador.IdRegistro, jogador.NomePersonagem, jogador.NomeCampanha));
        Assert.Equal(Hash[..JogadorService.TamanhoMarca], jogador.Marca);
        _convites.Verify(c => c.RegistrarAcessoAsync(1, It.IsAny<DateTime>()), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("codigo-errado")]
    public async Task EntrarAsync_CodigoErrado_Codigo20(string codigo)
    {
        Assert.Equal(BusinessErrors.ConviteInvalido.Code, await CodigoAsync(() => CriarService().EntrarAsync(codigo)));
    }

    [Fact]
    public async Task ConferirAcessoAsync_MestreTrocouOLink_Codigo21()
    {
        _convites.Setup(c => c.ObterPorRegistroAsync(100)).ReturnsAsync(new ConviteJogador
        {
            IdCampanha = 10, IdRegistro = 100, HashCodigo = JogadorService.HashDoCodigo("outro-codigo"),
        });
        Assert.Equal(BusinessErrors.AcessoJogadorExpirado.Code, await CodigoAsync(() => CriarService().ConferirAcessoAsync(_lilica)));
    }

    [Fact]
    public async Task ObterMesaAsync_FiltraEMarcaAMinha()
    {
        MesaAtiva();

        var visao = await CriarService().ObterMesaAsync(_lilica);

        Assert.Equal([1, 2], visao!.Participantes.Select(p => p.Id));
        Assert.True(visao.Participantes[0].EhMinha);
        Assert.Null(visao.Participantes[1].PvAtual);
    }

    [Fact]
    public async Task ObterMesaAsync_SemMesaAtiva_Nulo()
    {
        _mesas.Setup(m => m.MontarAtivaAsync(10)).ReturnsAsync((MesaServiceModel?)null);
        Assert.Null(await CriarService().ObterMesaAsync(_lilica));
    }

    [Fact]
    public async Task MoverMinhaFichaAsync_SoAPropria_EAvisa()
    {
        MesaAtiva();

        await CriarService().MoverMinhaFichaAsync(_lilica, 3, 4);

        _participantes.Verify(p => p.AtualizarPosicaoAsync(1, 3, 4), Times.Once);
        _participantes.Verify(p => p.AtualizarPosicaoAsync(2, It.IsAny<int?>(), It.IsAny<int?>()), Times.Never);
        _canal.Verify(c => c.Avisar(10), Times.Once);
    }

    [Fact]
    public async Task MoverMinhaFichaAsync_MovimentoTravado_Codigo22()
    {
        MesaAtiva(movimentoLivre: false);
        Assert.Equal(BusinessErrors.MovimentoBloqueado.Code, await CodigoAsync(() => CriarService().MoverMinhaFichaAsync(_lilica, 3, 4)));
    }

    [Fact]
    public async Task ObterImagemAsync_SoMapaERetratosVisiveis()
    {
        MesaAtiva();
        _imagens.Setup(i => i.ObterConteudoAsync(It.IsAny<int>())).ReturnsAsync(("image/png", new byte[] { 1 }));

        Assert.Equal("image/png", (await CriarService().ObterImagemAsync(_lilica, 300)).TipoConteudo);
        Assert.Equal("image/png", (await CriarService().ObterImagemAsync(_lilica, 301)).TipoConteudo);
        // 302 é o retrato de uma ficha oculta; 999 é qualquer outra imagem da galeria.
        Assert.Equal(BusinessErrors.ImagemNaoEncontrada.Code, await CodigoAsync(() => CriarService().ObterImagemAsync(_lilica, 302)));
        Assert.Equal(BusinessErrors.ImagemNaoEncontrada.Code, await CodigoAsync(() => CriarService().ObterImagemAsync(_lilica, 999)));
    }

    [Fact]
    public void CanalMesa_AvisaQuemOuveACampanha_EParaAoDescartar()
    {
        var canal = new CanalMesa();
        var (leitor, inscricao) = canal.Assinar(10, mestre: true);
        var (outra, inscricaoOutra) = canal.Assinar(20, mestre: true);

        canal.Avisar(10);

        Assert.True(leitor.TryRead(out var aviso));
        Assert.Equal("mesa", aviso!.Evento);
        Assert.False(outra.TryRead(out _));
        inscricao.Dispose();
        inscricaoOutra.Dispose();
        Assert.Equal(0, canal.ContarOuvintes(10));
    }

    [Fact]
    public void CanalMesa_SinalPrivadoSoChegaAoMestre()
    {
        var canal = new CanalMesa();
        var (mestre, inscricaoMestre) = canal.Assinar(10, mestre: true);
        var (jogador, inscricaoJogador) = canal.Assinar(10, mestre: false);

        canal.Sinalizar(10, new SinalMesaServiceModel { Tipo = "arrasto", IdParticipante = 2, Publico = false });
        Assert.True(mestre.TryRead(out var aviso));
        Assert.Equal("sinal", aviso!.Evento);
        Assert.Contains("\"idParticipante\":2", aviso.Dados);
        Assert.False(jogador.TryRead(out _));

        canal.Sinalizar(10, new SinalMesaServiceModel { Tipo = "ping", Coluna = 1, Linha = 1, Publico = true });
        Assert.True(jogador.TryRead(out _));
        inscricaoMestre.Dispose();
        inscricaoJogador.Dispose();
    }

    [Fact]
    public async Task SinalizarAsync_ArrastoUsaSempreAPropriaFicha_ERespeitaOMovimentoTravado()
    {
        MesaAtiva();
        SinalMesaServiceModel? enviado = null;
        _canal.Setup(c => c.Sinalizar(10, It.IsAny<SinalMesaServiceModel>())).Callback<int, SinalMesaServiceModel>((_, s) => enviado = s);
        var lilica = new JogadorLogadoServiceModel { IdCampanha = 10, IdRegistro = 100, NomePersonagem = "Lilica Lovejoy", Marca = _lilica.Marca };

        // Tentou arrastar o goblin (id 2): o sinal sai com a ficha dela (id 1).
        await CriarService().SinalizarAsync(lilica, new SinalMesaServiceModel { Tipo = "arrasto", IdParticipante = 2, Coluna = 3, Linha = 2 });
        Assert.Equal((1, true, "Lilica Lovejoy", 5), (enviado!.IdParticipante, enviado.Publico, enviado.Autor, enviado.IdMesa));

        await CriarService().SinalizarAsync(lilica, new SinalMesaServiceModel { Tipo = "reacao", Emoji = "🔥" });
        Assert.Equal(1, enviado.IdParticipante);

        MesaAtiva(movimentoLivre: false);
        Assert.Equal(BusinessErrors.MovimentoBloqueado.Code,
            await CodigoAsync(() => CriarService().SinalizarAsync(lilica, new SinalMesaServiceModel { Tipo = "arrasto", Coluna = 1, Linha = 1 })));
    }
}
