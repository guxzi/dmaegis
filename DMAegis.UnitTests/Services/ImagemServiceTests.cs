using DMAegis.Model;
using DMAegis.Repository;
using DMAegis.Services;
using DMAegis.Services.Exceptions;
using Moq;

namespace DMAegis.UnitTests.Services;

public class ImagemServiceTests
{
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46];
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00];
    private static readonly byte[] Gif = "GIF89a\0\0"u8.ToArray();
    private static readonly byte[] Webp = [.. "RIFF"u8.ToArray(), 0x24, 0x00, 0x00, 0x00, .. "WEBPVP8 "u8.ToArray()];
    private static readonly byte[] Texto = "Isto não é uma imagem."u8.ToArray();

    private readonly Mock<IImagemRegistroRepository> _imagens = new();
    private readonly Mock<IRegistroService> _registros = new();
    private readonly Mock<ICampanhaService> _campanhas = new();

    public ImagemServiceTests()
    {
        _campanhas.Setup(c => c.ObterDoUsuarioOuLancarAsync(1, 10)).ReturnsAsync(new Campanha { Id = 10, IdUsuario = 1 });
        _campanhas.Setup(c => c.ObterDoUsuarioOuLancarAsync(1, 20))
            .ThrowsAsync(new BusinessRuleException(BusinessErrors.CampanhaNaoEncontrada));
        _registros.Setup(r => r.ObterAsync(1, 100)).ReturnsAsync(new RegistroServiceModel { Id = 100, IdCampanha = 10, Nome = "Prisao" });
        _registros.Setup(r => r.ObterAsync(1, 200)).ThrowsAsync(new BusinessRuleException(BusinessErrors.RegistroNaoEncontrado));
        _imagens.Setup(i => i.ObterMetadadosAsync(50)).ReturnsAsync(new ImagemRegistro { Id = 50, IdRegistro = 100, Titulo = "Mapa", TipoConteudo = "image/jpeg" });
        _imagens.Setup(i => i.ObterMetadadosAsync(60)).ReturnsAsync(new ImagemRegistro { Id = 60, IdRegistro = 200, Titulo = "Alheio", TipoConteudo = "image/png" });
        _imagens.Setup(i => i.ObterMetadadosAsync(99)).ReturnsAsync((ImagemRegistro?)null);
        _imagens.Setup(i => i.ContarPorRegistroAsync(100)).ReturnsAsync(2);
    }

    private ImagemService CriarService() => new(_imagens.Object, _registros.Object, _campanhas.Object);

    [Fact]
    public async Task ListarDaCampanhaAsync_DevolveImagensComNomeETipoDoRegistro()
    {
        _imagens.Setup(i => i.ListarPorCampanhaAsync(10)).ReturnsAsync(
        [
            new ImagemDaCampanha { Id = 50, IdRegistro = 100, Titulo = "Retrato", TipoConteudo = "image/jpeg", Tamanho = 10, NomeRegistro = "Sken", IdTipoRegistro = 1 },
        ]);

        var lista = (await CriarService().ListarDaCampanhaAsync(1, 10)).ToList();

        var imagem = Assert.Single(lista);
        Assert.Equal(("Sken", 1, 100, "Retrato"), (imagem.NomeRegistro, imagem.IdTipoRegistro, imagem.IdRegistro, imagem.Titulo));
    }

    [Fact]
    public async Task ListarDaCampanhaAsync_CampanhaAlheia_Codigo5()
    {
        Assert.Equal(BusinessErrors.CampanhaNaoEncontrada.Code, await CodigoAsync(() => CriarService().ListarDaCampanhaAsync(1, 20)));
        _imagens.Verify(i => i.ListarPorCampanhaAsync(It.IsAny<int>()), Times.Never);
    }

    private static async Task<int> CodigoAsync(Func<Task> acao) =>
        (await Assert.ThrowsAsync<BusinessRuleException>(acao)).Code;

    [Fact]
    public void DetectarTipo_ReconheceAssinaturasEConteudoInvalido()
    {
        Assert.Equal("image/jpeg", ImagemService.DetectarTipo(Jpeg));
        Assert.Equal("image/png", ImagemService.DetectarTipo(Png));
        Assert.Equal("image/gif", ImagemService.DetectarTipo(Gif));
        Assert.Equal("image/webp", ImagemService.DetectarTipo(Webp));
        Assert.Null(ImagemService.DetectarTipo(Texto));
        Assert.Null(ImagemService.DetectarTipo([]));
    }

    [Fact]
    public async Task InserirAsync_TextoComExtensaoJpg_LancaCodigo14()
    {
        Assert.Equal(BusinessErrors.ImagemInvalida.Code,
            await CodigoAsync(() => CriarService().InserirAsync(1, 100, "mapa", Texto)));
        _imagens.Verify(i => i.InserirAsync(It.IsAny<ImagemRegistro>()), Times.Never);
    }

    [Fact]
    public async Task InserirAsync_MaisDe15MB_LancaCodigo15()
    {
        var grande = new byte[ImagemService.TamanhoMaximo + 1];
        Jpeg.CopyTo(grande, 0);

        Assert.Equal(BusinessErrors.ImagemGrandeDemais.Code,
            await CodigoAsync(() => CriarService().InserirAsync(1, 100, "mapa", grande)));
    }

    [Fact]
    public async Task InserirAsync_RegistroCom30Imagens_LancaCodigo16()
    {
        _imagens.Setup(i => i.ContarPorRegistroAsync(100)).ReturnsAsync(30);

        Assert.Equal(BusinessErrors.LimiteImagens.Code,
            await CodigoAsync(() => CriarService().InserirAsync(1, 100, "mapa", Jpeg)));
    }

    [Fact]
    public async Task InserirAsync_RegistroAlheio_LancaCodigo7ENaoGrava()
    {
        Assert.Equal(BusinessErrors.RegistroNaoEncontrado.Code,
            await CodigoAsync(() => CriarService().InserirAsync(1, 200, "mapa", Jpeg)));
        _imagens.Verify(i => i.InserirAsync(It.IsAny<ImagemRegistro>()), Times.Never);
    }

    [Fact]
    public async Task InserirAsync_TituloVazio_LancaCodigo2()
    {
        Assert.Equal(BusinessErrors.DadosInvalidos.Code,
            await CodigoAsync(() => CriarService().InserirAsync(1, 100, "   ", Jpeg)));
    }

    [Fact]
    public async Task InserirAsync_GravaTipoDetectadoTamanhoOrdemETituloAparado()
    {
        ImagemRegistro? gravada = null;
        _imagens.Setup(i => i.InserirAsync(It.IsAny<ImagemRegistro>()))
            .Callback<ImagemRegistro>(i => gravada = i).ReturnsAsync(70);

        var resultado = await CriarService().InserirAsync(1, 100, "  Prisao DM  ", Png);

        Assert.Equal("Prisao DM", gravada!.Titulo);
        Assert.Equal("image/png", gravada.TipoConteudo);
        Assert.Equal(Png.Length, gravada.Tamanho);
        Assert.Equal(3, gravada.Ordem);
        Assert.Equal(100, gravada.IdRegistro);
        Assert.Equal(70, resultado.Id);
    }

    [Fact]
    public async Task ObterArquivoAsync_ImagemDeRegistroAlheio_LancaCodigo13()
    {
        Assert.Equal(BusinessErrors.ImagemNaoEncontrada.Code, await CodigoAsync(() => CriarService().ObterArquivoAsync(1, 60)));
        _imagens.Verify(i => i.ObterConteudoAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ObterArquivoAsync_Inexistente_LancaCodigo13()
    {
        Assert.Equal(BusinessErrors.ImagemNaoEncontrada.Code, await CodigoAsync(() => CriarService().ObterArquivoAsync(1, 99)));
    }

    [Fact]
    public async Task ObterArquivoAsync_Propria_DevolveTipoEBytes()
    {
        _imagens.Setup(i => i.ObterConteudoAsync(50)).ReturnsAsync(("image/jpeg", Jpeg));

        var (tipo, conteudo) = await CriarService().ObterArquivoAsync(1, 50);

        Assert.Equal("image/jpeg", tipo);
        Assert.Equal(Jpeg, conteudo);
    }

    [Fact]
    public async Task ExcluirAsync_ImagemAlheia_NaoExclui()
    {
        Assert.Equal(BusinessErrors.ImagemNaoEncontrada.Code, await CodigoAsync(() => CriarService().ExcluirAsync(1, 60)));
        _imagens.Verify(i => i.ExcluirAsync(It.IsAny<int>()), Times.Never);
    }
}
