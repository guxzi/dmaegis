using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DMAegis.Services;
using DMAegis.Services.Exceptions;
using Moq;

namespace DMAegis.UnitTests.WebApi;

public class SessaoControllerTests
{
    private readonly ApiDeTeste _api = new();

    private static readonly SessaoServiceModel Sessao1 = new()
    {
        Id = 1, IdCampanha = 1, Numero = 1, Data = new DateTime(2026, 11, 20),
        Cenas = ["Ponte"], IdsRegistro = [2],
        Segredos = [new SegredoServiceModel { Id = 2, Texto = "O prefeito é um cultista." }],
    };

    private static async Task<JsonElement> LerEnvelopeAsync(HttpResponseMessage resposta) =>
        JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement;

    [Fact]
    public async Task Listar_SemLogin_Responde401()
    {
        var resposta = await _api.CriarCliente().GetAsync("/api/campanhas/1/sessoes");
        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task Inserir_SemNumero_UsaNumeroNulo()
    {
        _api.Sessoes.Setup(s => s.InserirAsync(7, 1, It.Is<InserirSessaoServiceModel>(d => d.Numero == null)))
            .ReturnsAsync(Sessao1);
        var cliente = await _api.CriarClienteLogadoAsync();

        var resposta = await cliente.PostAsJsonAsync("/api/campanhas/1/sessoes", new { });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        _api.Sessoes.Verify(s => s.InserirAsync(7, 1, It.Is<InserirSessaoServiceModel>(d => d.Numero == null)), Times.Once);
    }

    [Fact]
    public async Task Obter_DevolveDataNoFormatoIso()
    {
        _api.Sessoes.Setup(s => s.ObterAsync(7, 1)).ReturnsAsync(Sessao1);
        var cliente = await _api.CriarClienteLogadoAsync();

        var resposta = await cliente.GetAsync("/api/sessoes/1");

        var data = (await LerEnvelopeAsync(resposta)).GetProperty("data");
        Assert.Equal("2026-11-20", data.GetProperty("data").GetString());
        Assert.Equal("Ponte", data.GetProperty("cenas")[0].GetString());
        Assert.Equal("O prefeito é um cultista.", data.GetProperty("segredos")[0].GetProperty("texto").GetString());
    }

    [Fact]
    public async Task Atualizar_RepassaDataComoDateTime()
    {
        _api.Sessoes.Setup(s => s.AtualizarAsync(7, 1, It.IsAny<SalvarSessaoServiceModel>())).ReturnsAsync(Sessao1);
        var cliente = await _api.CriarClienteLogadoAsync();

        var resposta = await cliente.PutAsJsonAsync("/api/sessoes/1",
            new { numero = 1, data = "2026-11-20", cenas = new[] { "Ponte" }, idsRegistro = new[] { 2 } });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        _api.Sessoes.Verify(s => s.AtualizarAsync(7, 1, It.Is<SalvarSessaoServiceModel>(d =>
            d.Data == new DateTime(2026, 11, 20) && d.Cenas.Single() == "Ponte" && d.IdsRegistro.Single() == 2)), Times.Once);
    }

    [Fact]
    public async Task Atualizar_CenaLonga_Responde400Codigo2()
    {
        var cliente = await _api.CriarClienteLogadoAsync();

        var resposta = await cliente.PutAsJsonAsync("/api/sessoes/1",
            new { numero = 1, cenas = new[] { new string('c', 501) }, idsRegistro = Array.Empty<int>() });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal(2, (await LerEnvelopeAsync(resposta)).GetProperty("responseCode").GetInt32());
    }

    [Fact]
    public async Task CriarSegredo_Responde201()
    {
        _api.Sessoes.Setup(s => s.InserirSegredoAsync(7, 1, "A ponte está podre"))
            .ReturnsAsync(new SegredoServiceModel { Id = 9, Texto = "A ponte está podre", IdSessaoCriacao = 1 });
        var cliente = await _api.CriarClienteLogadoAsync();

        var resposta = await cliente.PostAsJsonAsync("/api/sessoes/1/segredos", new { texto = "A ponte está podre" });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.Equal(9, (await LerEnvelopeAsync(resposta)).GetProperty("data").GetProperty("id").GetInt32());
    }

    [Fact]
    public async Task Revelar_RepassaSegredoESessao()
    {
        _api.Sessoes.Setup(s => s.RevelarSegredoAsync(7, 2, 1))
            .ReturnsAsync(new SegredoServiceModel { Id = 2, Texto = "x", IdSessaoRevelacao = 1 });
        var cliente = await _api.CriarClienteLogadoAsync();

        var resposta = await cliente.PostAsJsonAsync("/api/segredos/2/revelar", new { idSessao = 1 });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        _api.Sessoes.Verify(s => s.RevelarSegredoAsync(7, 2, 1), Times.Once);
    }

    [Fact]
    public async Task Segredo_NaoEncontrado_Responde404Codigo11()
    {
        _api.Sessoes.Setup(s => s.OcultarSegredoAsync(7, 60))
            .ThrowsAsync(new BusinessRuleException(BusinessErrors.SegredoNaoEncontrado));
        var cliente = await _api.CriarClienteLogadoAsync();

        var resposta = await cliente.PostAsync("/api/segredos/60/ocultar", null);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Equal(11, (await LerEnvelopeAsync(resposta)).GetProperty("responseCode").GetInt32());
    }

    [Fact]
    public async Task Excluir_Responde200()
    {
        var cliente = await _api.CriarClienteLogadoAsync();

        var resposta = await cliente.DeleteAsync("/api/sessoes/1");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        _api.Sessoes.Verify(s => s.ExcluirAsync(7, 1), Times.Once);
    }
}
