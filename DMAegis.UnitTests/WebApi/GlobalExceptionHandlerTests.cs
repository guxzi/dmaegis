using System.Text.Json;
using DMAegis.Services.Exceptions;
using DMAegis.WebApi.ApiModels;
using DMAegis.WebApi.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace DMAegis.UnitTests.WebApi;

public class GlobalExceptionHandlerTests
{
    private static readonly JsonSerializerOptions OpcoesJson = new(JsonSerializerDefaults.Web);

    private static async Task<(HttpContext Contexto, string Corpo)> ExecutarAsync(Exception excecao)
    {
        var contexto = new DefaultHttpContext();
        contexto.Response.Body = new MemoryStream();
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);

        var tratou = await handler.TryHandleAsync(contexto, excecao, CancellationToken.None);

        Assert.True(tratou);
        contexto.Response.Body.Position = 0;
        var corpo = await new StreamReader(contexto.Response.Body).ReadToEndAsync();
        return (contexto, corpo);
    }

    [Fact]
    public async Task TryHandleAsync_ExcecaoGenerica_Responde500ComEnvelopeErro()
    {
        var (contexto, corpo) = await ExecutarAsync(new InvalidOperationException("detalhe interno"));

        var envelope = JsonSerializer.Deserialize<ApiResponse<object>>(corpo, OpcoesJson)!;
        Assert.Equal(500, contexto.Response.StatusCode);
        Assert.Equal(ResponseCodes.ERRO, envelope.ResponseCode);
        Assert.Equal("Erro inesperado. Tente novamente.", envelope.ErrorMessage);
        Assert.DoesNotContain("detalhe interno", corpo);
        Assert.DoesNotContain("at ", corpo);
    }

    [Fact]
    public async Task TryHandleAsync_BusinessRuleException_Responde400ComCodigoDela()
    {
        var (contexto, corpo) = await ExecutarAsync(new BusinessRuleException((99, "Regra X")));

        var envelope = JsonSerializer.Deserialize<ApiResponse<object>>(corpo, OpcoesJson)!;
        Assert.Equal(400, contexto.Response.StatusCode);
        Assert.Equal(99, envelope.ResponseCode);
        Assert.Equal("Regra X", envelope.ErrorMessage);
    }
}
