using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DMAegis.Services;
using DMAegis.Services.Exceptions;
using Moq;

namespace DMAegis.UnitTests.WebApi;

public class MesaControllerTests
{
    private readonly ApiDeTeste _api = new();

    private static async Task<JsonElement> LerEnvelopeAsync(HttpResponseMessage resposta) =>
        JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement;

    [Fact]
    public async Task Obter_SemLogin_401()
    {
        var resposta = await _api.CriarCliente().GetAsync("/api/mesas/5");
        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task Obter_MesaAlheia_404ComCodigo17()
    {
        _api.Mesas.Setup(s => s.ObterAsync(7, 5)).ThrowsAsync(new BusinessRuleException(BusinessErrors.MesaNaoEncontrada));
        var cliente = await _api.CriarClienteLogadoAsync();

        var resposta = await cliente.GetAsync("/api/mesas/5");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Equal(17, (await LerEnvelopeAsync(resposta)).GetProperty("responseCode").GetInt32());
    }

    [Fact]
    public async Task Inserir_SemNome_400()
    {
        var cliente = await _api.CriarClienteLogadoAsync();

        var resposta = await cliente.PostAsJsonAsync("/api/campanhas/10/mesas", new { nome = " ", tamanhoCelula = 70 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        _api.Mesas.Verify(s => s.InserirAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<SalvarMesaServiceModel>()), Times.Never);
    }

    [Fact]
    public async Task Inserir_Valida_201ComAMesa()
    {
        _api.Mesas.Setup(s => s.InserirAsync(7, 10, It.Is<SalvarMesaServiceModel>(m => m.Nome == "Ponte" && m.TamanhoCelula == 64)))
            .ReturnsAsync(new MesaServiceModel { Id = 3, IdCampanha = 10, Nome = "Ponte", TamanhoCelula = 64, Ativa = true });
        var cliente = await _api.CriarClienteLogadoAsync();

        var resposta = await cliente.PostAsJsonAsync("/api/campanhas/10/mesas", new { nome = "Ponte", tamanhoCelula = 64 });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var dados = (await LerEnvelopeAsync(resposta)).GetProperty("data");
        Assert.Equal((3, true), (dados.GetProperty("id").GetInt32(), dados.GetProperty("ativa").GetBoolean()));
    }

    [Fact]
    public async Task Mover_RepassaColunaELinha()
    {
        var cliente = await _api.CriarClienteLogadoAsync();

        var resposta = await cliente.PutAsJsonAsync("/api/participantes/8/posicao", new { coluna = 4, linha = 2 });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        _api.Mesas.Verify(s => s.MoverParticipanteAsync(7, 8, 4, 2), Times.Once);
    }

    [Fact]
    public async Task AtualizarParticipante_TamanhoForaDaFaixa_400()
    {
        var cliente = await _api.CriarClienteLogadoAsync();

        var resposta = await cliente.PutAsJsonAsync("/api/participantes/8", new { nome = "Goblin", tamanho = 9 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task VisaoJogador_DevolveEstadoFiltrado()
    {
        _api.Mesas.Setup(s => s.ObterVisaoJogadorAsync(7, 5)).ReturnsAsync(new VisaoJogadorServiceModel
        {
            IdMesa = 5,
            Participantes = [new ParticipanteVisaoServiceModel { Id = 2, Nome = "Goblin 1", EstadoSaude = "Sangrando" }],
        });
        var cliente = await _api.CriarClienteLogadoAsync();

        var resposta = await cliente.GetAsync("/api/mesas/5/visao-jogador");

        var ficha = (await LerEnvelopeAsync(resposta)).GetProperty("data").GetProperty("participantes")[0];
        Assert.Equal("Sangrando", ficha.GetProperty("estadoSaude").GetString());
        Assert.Equal(JsonValueKind.Null, ficha.GetProperty("pvAtual").ValueKind);
    }
}
