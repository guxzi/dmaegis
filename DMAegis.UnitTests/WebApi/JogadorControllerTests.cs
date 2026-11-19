using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DMAegis.Services;
using DMAegis.Services.Exceptions;
using Moq;

namespace DMAegis.UnitTests.WebApi;

public class JogadorControllerTests
{
    private readonly ApiDeTeste _api = new();

    private static async Task<JsonElement> LerEnvelopeAsync(HttpResponseMessage resposta) =>
        JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement;

    private async Task<HttpClient> EntrarComoJogadorAsync()
    {
        _api.Jogadores.Setup(s => s.EntrarAsync("codigo-bom")).ReturnsAsync(new JogadorLogadoServiceModel
        {
            IdCampanha = 10, IdRegistro = 100, NomePersonagem = "Lilica Lovejoy", NomeCampanha = "Teste", Marca = "abcdef0123456789",
        });
        var cliente = _api.CriarCliente();
        await RenovarCsrfJogadorAsync(cliente);
        var resposta = await cliente.PostAsJsonAsync("/api/jogador/entrar", new { codigo = "codigo-bom" });
        resposta.EnsureSuccessStatusCode();
        // Depois de entrar, a identidade mudou: o token precisa ser pedido de novo.
        await RenovarCsrfJogadorAsync(cliente);
        return cliente;
    }

    private static async Task RenovarCsrfJogadorAsync(HttpClient cliente)
    {
        var resposta = await cliente.GetAsync("/api/jogador/csrf");
        resposta.EnsureSuccessStatusCode();
        var cookie = resposta.Headers.GetValues("Set-Cookie").Single(c => c.StartsWith("XSRF-TOKEN-JOGADOR="));
        var token = Uri.UnescapeDataString(cookie.Split(';')[0]["XSRF-TOKEN-JOGADOR=".Length..]);
        cliente.DefaultRequestHeaders.Remove("X-XSRF-TOKEN");
        cliente.DefaultRequestHeaders.Add("X-XSRF-TOKEN", token);
    }

    [Fact]
    public async Task Entrar_CodigoInvalido_404ComCodigo20()
    {
        _api.Jogadores.Setup(s => s.EntrarAsync(It.IsAny<string>())).ThrowsAsync(new BusinessRuleException(BusinessErrors.ConviteInvalido));
        var cliente = _api.CriarCliente();
        await RenovarCsrfJogadorAsync(cliente);

        var resposta = await cliente.PostAsJsonAsync("/api/jogador/entrar", new { codigo = "x" });

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Equal(20, (await LerEnvelopeAsync(resposta)).GetProperty("responseCode").GetInt32());
    }

    [Fact]
    public async Task Jogador_VeAMesaComOsDadosDoProprioCookie()
    {
        _api.Jogadores.Setup(s => s.ObterMesaAsync(It.Is<JogadorLogadoServiceModel>(j => j.IdCampanha == 10 && j.IdRegistro == 100 && j.Marca == "abcdef0123456789")))
            .ReturnsAsync(new VisaoJogadorServiceModel { IdMesa = 5, Nome = "Ponte" });
        var cliente = await EntrarComoJogadorAsync();

        var resposta = await cliente.GetAsync("/api/jogador/mesa");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal("Ponte", (await LerEnvelopeAsync(resposta)).GetProperty("data").GetProperty("nome").GetString());
    }

    [Fact]
    public async Task CookieDeJogador_NaoAbreRotasDoMestre()
    {
        var cliente = await EntrarComoJogadorAsync();

        Assert.Equal(HttpStatusCode.Unauthorized, (await cliente.GetAsync("/api/mesas/5")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await cliente.GetAsync("/api/campanhas/10/convites")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await cliente.GetAsync("/api/imagens/300")).StatusCode);
    }

    [Fact]
    public async Task CookieDoMestre_NaoAbreRotasDeJogador()
    {
        var cliente = await _api.CriarClienteLogadoAsync();

        Assert.Equal(HttpStatusCode.Unauthorized, (await cliente.GetAsync("/api/jogador/mesa")).StatusCode);
    }

    [Fact]
    public async Task SemCookie_RotasDeJogador401()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await _api.CriarCliente().GetAsync("/api/jogador/mesa")).StatusCode);
    }

    [Fact]
    public async Task Mover_PosicaoNegativa_400()
    {
        var cliente = await EntrarComoJogadorAsync();

        var resposta = await cliente.PutAsJsonAsync("/api/jogador/ficha/posicao", new { coluna = -1, linha = 2 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal(2, (await LerEnvelopeAsync(resposta)).GetProperty("responseCode").GetInt32());
        _api.Jogadores.Verify(s => s.MoverMinhaFichaAsync(It.IsAny<JogadorLogadoServiceModel>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Mover_ComOTokenDoJogador_ChegaAoService()
    {
        var cliente = await EntrarComoJogadorAsync();

        var resposta = await cliente.PutAsJsonAsync("/api/jogador/ficha/posicao", new { coluna = 3, linha = 4 });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        _api.Jogadores.Verify(s => s.MoverMinhaFichaAsync(It.Is<JogadorLogadoServiceModel>(j => j.IdRegistro == 100), 3, 4), Times.Once);
    }

    [Fact]
    public async Task Mover_ComTokenAnonimoDeAntesDeEntrar_Recusado()
    {
        _api.Jogadores.Setup(s => s.EntrarAsync("codigo-bom")).ReturnsAsync(new JogadorLogadoServiceModel
        {
            IdCampanha = 10, IdRegistro = 100, NomePersonagem = "Lilica Lovejoy", Marca = "abcdef0123456789",
        });
        var cliente = _api.CriarCliente();
        await RenovarCsrfJogadorAsync(cliente);
        (await cliente.PostAsJsonAsync("/api/jogador/entrar", new { codigo = "codigo-bom" })).EnsureSuccessStatusCode();

        var resposta = await cliente.PutAsJsonAsync("/api/jogador/ficha/posicao", new { coluna = 3, linha = 4 });

        Assert.NotEqual(HttpStatusCode.OK, resposta.StatusCode);
        _api.Jogadores.Verify(s => s.MoverMinhaFichaAsync(It.IsAny<JogadorLogadoServiceModel>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GerarConvite_DoMestre_DevolveOCodigo()
    {
        _api.Jogadores.Setup(s => s.GerarConviteAsync(7, 100)).ReturnsAsync("novo-codigo");
        var cliente = await _api.CriarClienteLogadoAsync();

        var resposta = await cliente.PostAsync("/api/registros/100/convite", null);

        Assert.Equal("novo-codigo", (await LerEnvelopeAsync(resposta)).GetProperty("data").GetProperty("codigo").GetString());
    }
}
