using DMAegis.Services.Exceptions;
using DMAegis.WebApi.ApiModels;
using Microsoft.AspNetCore.Diagnostics;

namespace DMAegis.WebApi.Middlewares;

/// <summary>
/// Converte exceções no envelope padrão. Regra de negócio → 400 com o código dela;
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
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            envelope = ApiResponse<object>.Erro(regra.Code, regra.Mensagem);
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
}
