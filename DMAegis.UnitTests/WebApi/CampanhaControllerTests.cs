using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DMAegis.Services;
using DMAegis.Services.Exceptions;
using Moq;

namespace DMAegis.UnitTests.WebApi;

public class CampanhaControllerTests
{
    private readonly ApiDeTeste _api = new();

    private static readonly CampanhaServiceModel Mina = new()
    {
        Id = 1, IdSistema = 1, NomeSistema = "D&D 5e (2024)", Nome = "A Mina Perdida",
    };

    private static async Task<JsonElement> LerEnvelopeAsync(HttpResponseMessage resposta) =>
        JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement;

    [Fact]
    public async Task Listar_SemLogin_Responde401()
    {
        var resposta = await _api.CriarCliente().GetAsync("/api/campanhas");
        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task Sistemas_SemLogin_Responde401()
    {
        var resposta = await _api.CriarCliente().GetAsync("/api/sistemas");
        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task Listar_PassaOIdDoUsuarioLogadoParaOService()
    {
        _api.Campanhas.Setup(s => s.ListarAsync(7)).ReturnsAsync([Mina]);
        var cliente = await _api.CriarClienteLogadoAsync();

        var resposta = await cliente.GetAsync("/api/campanhas");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var data = (await LerEnvelopeAsync(resposta)).GetProperty("data");
        Assert.Equal("A Mina Perdida", data[0].GetProperty("nome").GetString());
        Assert.Equal("D&D 5e (2024)", data[0].GetProperty("nomeSistema").GetString());
        _api.Campanhas.Verify(s => s.ListarAsync(7), Times.Once);
    }

    [Fact]
    public async Task Inserir_Valido_Responde201ComCampanha()
    {
        _api.Campanhas.Setup(s => s.InserirAsync(7, It.Is<InserirCampanhaServiceModel>(d =>
            d.Nome == "A Mina Perdida" && d.IdSistema == 1))).ReturnsAsync(Mina);
        var cliente = await _api.CriarClienteLogadoAsync();

        var resposta = await cliente.PostAsJsonAsync("/api/campanhas", new { idSistema = 1, nome = "A Mina Perdida" });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.Equal(1, (await LerEnvelopeAsync(resposta)).GetProperty("data").GetProperty("id").GetInt32());
    }

    [Fact]
    public async Task Inserir_NomeVazio_Responde400Codigo2()
    {
        var cliente = await _api.CriarClienteLogadoAsync();

        var resposta = await cliente.PostAsJsonAsync("/api/campanhas", new { idSistema = 1, nome = "  " });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var envelope = await LerEnvelopeAsync(resposta);
        Assert.Equal(2, envelope.GetProperty("responseCode").GetInt32());
        Assert.Contains("Informe o nome da campanha.", envelope.GetProperty("errorMessage").GetString());
    }

    [Fact]
    public async Task Obter_CampanhaNaoEncontrada_Responde404Codigo5()
    {
        _api.Campanhas.Setup(s => s.ObterAsync(7, 20))
            .ThrowsAsync(new BusinessRuleException(BusinessErrors.CampanhaNaoEncontrada));
        var cliente = await _api.CriarClienteLogadoAsync();

        var resposta = await cliente.GetAsync("/api/campanhas/20");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Equal(5, (await LerEnvelopeAsync(resposta)).GetProperty("responseCode").GetInt32());
    }

    [Fact]
    public async Task Atualizar_IgnoraIdSistemaDoCorpo()
    {
        _api.Campanhas.Setup(s => s.AtualizarAsync(7, 1, It.IsAny<AtualizarCampanhaServiceModel>())).ReturnsAsync(Mina);
        var cliente = await _api.CriarClienteLogadoAsync();

        var resposta = await cliente.PutAsJsonAsync("/api/campanhas/1", new { nome = "Novo", idSistema = 2 });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        _api.Campanhas.Verify(s => s.AtualizarAsync(7, 1,
            It.Is<AtualizarCampanhaServiceModel>(d => d.Nome == "Novo")), Times.Once);
    }

    [Fact]
    public async Task Arquivar_ChamaServiceComTrue()
    {
        _api.Campanhas.Setup(s => s.DefinirArquivadaAsync(7, 1, true)).ReturnsAsync(Mina);
        var cliente = await _api.CriarClienteLogadoAsync();

        var resposta = await cliente.PostAsync("/api/campanhas/1/arquivar", null);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        _api.Campanhas.Verify(s => s.DefinirArquivadaAsync(7, 1, true), Times.Once);
    }

    [Fact]
    public async Task Excluir_Responde200ComDataNull()
    {
        var cliente = await _api.CriarClienteLogadoAsync();

        var resposta = await cliente.DeleteAsync("/api/campanhas/1");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal(JsonValueKind.Null, (await LerEnvelopeAsync(resposta)).GetProperty("data").ValueKind);
        _api.Campanhas.Verify(s => s.ExcluirAsync(7, 1), Times.Once);
    }

    [Fact]
    public async Task Atualizar_SemCsrf_Responde400()
    {
        var cliente = await _api.CriarClienteLogadoAsync();
        cliente.DefaultRequestHeaders.Remove("X-XSRF-TOKEN");

        var resposta = await cliente.PutAsJsonAsync("/api/campanhas/1", new { nome = "Novo" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        _api.Campanhas.Verify(s => s.AtualizarAsync(It.IsAny<int>(), It.IsAny<int>(),
            It.IsAny<AtualizarCampanhaServiceModel>()), Times.Never);
    }
}
