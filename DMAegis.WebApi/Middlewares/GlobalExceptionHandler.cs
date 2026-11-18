using DMAegis.Services.Exceptions;
using DMAegis.WebApi.ApiModels;
using Microsoft.AspNetCore.Diagnostics;

namespace DMAegis.WebApi.Middlewares;

/// <summary>
/// Converte exceções no envelope padrão. Regra de negócio → status de StatusErroNegocio com o código dela;
/// qualquer outra → 500 com mensagem genérica (o detalhe vai só para o log).
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    public const string MensagemErroInesperado = "Erro inesperado. Tente novamente.";

    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        ApiResponse<object> envelope;

        if (exception is BusinessRuleException regra)
        {
            httpContext.Response.StatusCode = StatusErroNegocio.Obter(regra.Code);
            envelope = ApiResponse<object>.Erro(regra.Code, regra.Mensagem);
        }
        else if (CorpoGrandeDemais(exception))
        {
            // Upload acima do limite: o Kestrel (413) ou o leitor de multipart cortam antes do Service.
            httpContext.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
            envelope = ApiResponse<object>.Erro(BusinessErrors.ImagemGrandeDemais.Code, BusinessErrors.ImagemGrandeDemais.DefaultMessage);
        }
        else
        {
            _logger.LogError(exception, "Erro não tratado em {Caminho}", httpContext.Request.Path);
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            envelope = ApiResponse<object>.Erro(ResponseCodes.ERRO, MensagemErroInesperado);
        }

        await httpContext.Response.WriteAsJsonAsync(envelope, cancellationToken);
        return true;
    }

    private static bool CorpoGrandeDemais(Exception exception) => exception switch
    {
        BadHttpRequestException { StatusCode: StatusCodes.Status413PayloadTooLarge } => true,
        InvalidDataException dados => dados.Message.Contains("limit", StringComparison.OrdinalIgnoreCase),
        _ => false,
    };
}
