using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using DMAegis.Services;
using DMAegis.Services.Exceptions;
using Moq;

namespace DMAegis.UnitTests.WebApi;

public class ImagemControllerTests
{
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10];
    private readonly ApiDeTeste _api = new();

    private static async Task<JsonElement> LerEnvelopeAsync(HttpResponseMessage resposta) =>
        JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement;

    private static MultipartFormDataContent Formulario(byte[] bytes, string nomeArquivo, string? titulo = null)
    {
        var form = new MultipartFormDataContent();
        var arquivo = new ByteArrayContent(bytes);
        arquivo.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        form.Add(arquivo, "arquivo", nomeArquivo);
        if (titulo is not null) form.Add(new StringContent(titulo), "titulo");
        return form;
    }

    [Fact]
    public async Task Listar_SemLogin_401()
    {
        var resposta = await _api.CriarCliente().GetAsync("/api/registros/4/imagens");
        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task Upload_SemTitulo_UsaNomeDoArquivoERepassaBytes()
    {
        _api.Imagens.Setup(s => s.InserirAsync(7, 4, "Prisao do Porto - DM Version", It.IsAny<byte[]>()))
            .ReturnsAsync(new ImagemServiceModel { Id = 9, IdRegistro = 4, Titulo = "Prisao do Porto - DM Version", TipoConteudo = "image/jpeg", Tamanho = 6 });
        var cliente = await _api.CriarClienteLogadoAsync();

        var resposta = await cliente.PostAsync("/api/registros/4/imagens", Formulario(Jpeg, "Prisao do Porto - DM Version.jpg"));

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.Equal(9, (await LerEnvelopeAsync(resposta)).GetProperty("data").GetProperty("id").GetInt32());
        _api.Imagens.Verify(s => s.InserirAsync(7, 4, "Prisao do Porto - DM Version",
            It.Is<byte[]>(b => b.SequenceEqual(Jpeg))), Times.Once);
    }

    [Fact]
    public async Task Upload_ComTitulo_UsaOTitulo()
    {
        _api.Imagens.Setup(s => s.InserirAsync(7, 4, "Prisão (mestre)", It.IsAny<byte[]>()))
            .ReturnsAsync(new ImagemServiceModel { Id = 9, Titulo = "Prisão (mestre)" });
        var cliente = await _api.CriarClienteLogadoAsync();

        var resposta = await cliente.PostAsync("/api/registros/4/imagens", Formulario(Jpeg, "x.jpg", "Prisão (mestre)"));

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    [Fact]
    public async Task Upload_SemArquivo_400Codigo2()
    {
        var cliente = await _api.CriarClienteLogadoAsync();
        var form = new MultipartFormDataContent { { new StringContent("só título"), "titulo" } };

        var resposta = await cliente.PostAsync("/api/registros/4/imagens", form);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal(2, (await LerEnvelopeAsync(resposta)).GetProperty("responseCode").GetInt32());
    }

    [Fact]
    public async Task Upload_Maior16MB_413ComEnvelopeCodigo15()
    {
        var cliente = await _api.CriarClienteLogadoAsync();
        var grande = new byte[16 * 1024 * 1024 + 1];

        var resposta = await cliente.PostAsync("/api/registros/4/imagens", Formulario(grande, "enorme.jpg"));

        Assert.True(resposta.StatusCode == HttpStatusCode.RequestEntityTooLarge, await resposta.Content.ReadAsStringAsync());
        var envelope = await LerEnvelopeAsync(resposta);
        Assert.Equal(15, envelope.GetProperty("responseCode").GetInt32());
        Assert.Equal("A imagem pode ter até 15 MB.", envelope.GetProperty("errorMessage").GetString());
        _api.Imagens.Verify(s => s.InserirAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<byte[]>()), Times.Never);
    }

    [Fact]
    public async Task Obter_DevolveBytesContentTypeECacheImutavel()
    {
        _api.Imagens.Setup(s => s.ObterArquivoAsync(7, 9)).ReturnsAsync(("image/jpeg", Jpeg));
        var cliente = await _api.CriarClienteLogadoAsync();

        var resposta = await cliente.GetAsync("/api/imagens/9");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal("image/jpeg", resposta.Content.Headers.ContentType!.MediaType);
        Assert.Equal(Jpeg, await resposta.Content.ReadAsByteArrayAsync());
        var cache = resposta.Headers.CacheControl!;
        Assert.True(cache.Private);
        Assert.Equal(TimeSpan.FromDays(365), cache.MaxAge);
        Assert.Contains("immutable", resposta.Headers.GetValues("Cache-Control").Single());
    }

    [Fact]
    public async Task Obter_Alheia_404Codigo13()
    {
        _api.Imagens.Setup(s => s.ObterArquivoAsync(7, 60)).ThrowsAsync(new BusinessRuleException(BusinessErrors.ImagemNaoEncontrada));
        var cliente = await _api.CriarClienteLogadoAsync();

        var resposta = await cliente.GetAsync("/api/imagens/60");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Equal(13, (await LerEnvelopeAsync(resposta)).GetProperty("responseCode").GetInt32());
    }

    [Fact]
    public async Task Listar_DevolveMetadados()
    {
        _api.Imagens.Setup(s => s.ListarAsync(7, 4)).ReturnsAsync(
            [new ImagemServiceModel { Id = 9, IdRegistro = 4, Titulo = "Prisao", TipoConteudo = "image/jpeg", Tamanho = 1234 }]);
        var cliente = await _api.CriarClienteLogadoAsync();

        var data = (await LerEnvelopeAsync(await cliente.GetAsync("/api/registros/4/imagens"))).GetProperty("data");

        Assert.Equal("Prisao", data[0].GetProperty("titulo").GetString());
        Assert.Equal(1234, data[0].GetProperty("tamanho").GetInt32());
    }

    [Fact]
    public async Task Excluir_Responde200()
    {
        var cliente = await _api.CriarClienteLogadoAsync();

        var resposta = await cliente.DeleteAsync("/api/imagens/9");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        _api.Imagens.Verify(s => s.ExcluirAsync(7, 9), Times.Once);
    }
}
