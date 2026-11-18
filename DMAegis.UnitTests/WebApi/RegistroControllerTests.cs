using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DMAegis.Repository;
using DMAegis.Services;
using DMAegis.Services.Exceptions;
using Moq;

namespace DMAegis.UnitTests.WebApi;

public class RegistroControllerTests
{
    private readonly ApiDeTeste _api = new();

    private static readonly RegistroServiceModel Goblin = new()
    {
        Id = 4, IdCampanha = 1, IdTipoRegistro = 3, Nome = "Goblin",
        Atributos = [new() { Chave = "CA", Valor = "15" }],
        Tags = [new() { Id = 1, Nome = "estrada" }],
    };

    private static async Task<JsonElement> LerEnvelopeAsync(HttpResponseMessage resposta) =>
        JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement;

    private static object CorpoValido() => new
    {
        idTipoRegistro = 3,
        nome = "Goblin",
        atributos = new[] { new { chave = "CA", valor = "15" } },
        tags = new[] { "estrada" },
    };

    [Fact]
    public async Task Listar_SemLogin_Responde401()
    {
        var resposta = await _api.CriarCliente().GetAsync("/api/campanhas/1/registros");
        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task Listar_RepassaFiltrosDaQuery()
    {
        FiltroRegistro? recebido = null;
        _api.Registros.Setup(s => s.ListarAsync(7, 1, It.IsAny<FiltroRegistro>()))
            .Callback<int, int, FiltroRegistro>((_, _, f) => recebido = f)
            .ReturnsAsync([new RegistroResumoServiceModel { Id = 4, IdTipoRegistro = 3, Nome = "Goblin" }]);
        var cliente = await _api.CriarClienteLogadoAsync();

        var resposta = await cliente.GetAsync("/api/campanhas/1/registros?idTipoRegistro=3&idTag=5&texto=ulm");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal((3, 5, "ulm"), (recebido!.IdTipoRegistro, recebido.IdTag, recebido.Texto));
        Assert.Equal("Goblin", (await LerEnvelopeAsync(resposta)).GetProperty("data")[0].GetProperty("nome").GetString());
    }

    [Fact]
    public async Task Inserir_Valido_Responde201()
    {
        _api.Registros.Setup(s => s.InserirAsync(7, 1, It.Is<SalvarRegistroServiceModel>(d =>
                d.Nome == "Goblin" && d.Atributos.Single().Chave == "CA" && d.Tags.Single() == "estrada")))
            .ReturnsAsync(Goblin);
        var cliente = await _api.CriarClienteLogadoAsync();

        var resposta = await cliente.PostAsJsonAsync("/api/campanhas/1/registros", CorpoValido());

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var data = (await LerEnvelopeAsync(resposta)).GetProperty("data");
        Assert.Equal("CA", data.GetProperty("atributos")[0].GetProperty("chave").GetString());
        Assert.Equal("estrada", data.GetProperty("tags")[0].GetProperty("nome").GetString());
    }

    [Fact]
    public async Task Inserir_ChaveRepetida_Responde400Codigo2()
    {
        var cliente = await _api.CriarClienteLogadoAsync();

        var resposta = await cliente.PostAsJsonAsync("/api/campanhas/1/registros", new
        {
            idTipoRegistro = 3,
            nome = "Goblin",
            atributos = new[] { new { chave = "CA", valor = "15" }, new { chave = "ca", valor = "16" } },
            tags = Array.Empty<string>(),
        });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var envelope = await LerEnvelopeAsync(resposta);
        Assert.Equal(2, envelope.GetProperty("responseCode").GetInt32());
        Assert.Contains("Atributo repetido: ca.", envelope.GetProperty("errorMessage").GetString());
    }

    [Fact]
    public async Task Obter_RegistroNaoEncontrado_Responde404Codigo7()
    {
        _api.Registros.Setup(s => s.ObterAsync(7, 200))
            .ThrowsAsync(new BusinessRuleException(BusinessErrors.RegistroNaoEncontrado));
        var cliente = await _api.CriarClienteLogadoAsync();

        var resposta = await cliente.GetAsync("/api/registros/200");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Equal(7, (await LerEnvelopeAsync(resposta)).GetProperty("responseCode").GetInt32());
    }

    [Fact]
    public async Task Atualizar_RepassaIdDoRegistro()
    {
        _api.Registros.Setup(s => s.AtualizarAsync(7, 4, It.IsAny<SalvarRegistroServiceModel>())).ReturnsAsync(Goblin);
        var cliente = await _api.CriarClienteLogadoAsync();

        var resposta = await cliente.PutAsJsonAsync("/api/registros/4", CorpoValido());

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        _api.Registros.Verify(s => s.AtualizarAsync(7, 4, It.IsAny<SalvarRegistroServiceModel>()), Times.Once);
    }

    [Fact]
    public async Task Modelos_RepassaSistemaETipo()
    {
        _api.Registros.Setup(s => s.ListarModelosAsync(2, 3)).ReturnsAsync(
        [
            new ModeloAtributoServiceModel { IdTipoRegistro = 3, Chave = "Limiar Maior", Ordem = 4 },
        ]);
        var cliente = await _api.CriarClienteLogadoAsync();

        var resposta = await cliente.GetAsync("/api/sistemas/2/modelos-atributo?idTipoRegistro=3");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal("Limiar Maior",
            (await LerEnvelopeAsync(resposta)).GetProperty("data")[0].GetProperty("chave").GetString());
    }

    [Fact]
    public async Task Tipos_DevolveLista()
    {
        _api.Registros.Setup(s => s.ListarTiposAsync()).ReturnsAsync([new TipoRegistroServiceModel { Id = 3, Nome = "Criatura" }]);
        var cliente = await _api.CriarClienteLogadoAsync();

        var resposta = await cliente.GetAsync("/api/tipos-registro");

        Assert.Equal("Criatura", (await LerEnvelopeAsync(resposta)).GetProperty("data")[0].GetProperty("nome").GetString());
    }

    [Fact]
    public async Task Tags_RepassaUsuarioECampanha()
    {
        _api.Registros.Setup(s => s.ListarTagsAsync(7, 1)).ReturnsAsync([new TagServiceModel { Id = 1, Nome = "vila" }]);
        var cliente = await _api.CriarClienteLogadoAsync();

        var resposta = await cliente.GetAsync("/api/campanhas/1/tags");

        Assert.Equal("vila", (await LerEnvelopeAsync(resposta)).GetProperty("data")[0].GetProperty("nome").GetString());
    }

    [Fact]
    public async Task Busca_RepassaTermo()
    {
        _api.Registros.Setup(s => s.BuscarAsync(7, 1, "gob"))
            .ReturnsAsync([new RegistroResumoServiceModel { Id = 4, IdTipoRegistro = 3, Nome = "Goblin" }]);
        var cliente = await _api.CriarClienteLogadoAsync();

        var resposta = await cliente.GetAsync("/api/campanhas/1/busca?q=gob");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal("Goblin", (await LerEnvelopeAsync(resposta)).GetProperty("data")[0].GetProperty("nome").GetString());
    }

    [Fact]
    public async Task Excluir_Responde200()
    {
        var cliente = await _api.CriarClienteLogadoAsync();

        var resposta = await cliente.DeleteAsync("/api/registros/4");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        _api.Registros.Verify(s => s.ExcluirAsync(7, 4), Times.Once);
    }
}
