using DMAegis.WebApi.ApiModels;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace DMAegis.WebApi.Middlewares;

/// <summary>
/// Exige o token CSRF (cabeçalho X-XSRF-TOKEN) em toda requisição que altera dados
/// (POST, PUT, PATCH, DELETE). Ações marcadas com [IgnoreAntiforgeryToken] ficam de fora.
/// Faz o papel do AutoValidateAntiforgeryToken, que depende dos serviços de Views do MVC.
/// </summary>
public class ValidarCsrfFilter : IAsyncAuthorizationFilter
{
    public const string MensagemTokenInvalido = "Sessão de segurança expirada. Recarregue a página e tente de novo.";

    private static readonly string[] MetodosSeguros = ["GET", "HEAD", "OPTIONS", "TRACE"];

    private readonly IAntiforgery _antiforgery;

    public ValidarCsrfFilter(IAntiforgery antiforgery)
    {
        _antiforgery = antiforgery;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var metodo = context.HttpContext.Request.Method;
        if (MetodosSeguros.Contains(metodo, StringComparer.OrdinalIgnoreCase))
            return;

        // O [IgnoreAntiforgeryToken] mais próximo da ação decide.
        var politica = context.ActionDescriptor.EndpointMetadata.OfType<IAntiforgeryPolicy>().LastOrDefault();
        if (politica is IgnoreAntiforgeryTokenAttribute)
            return;

        try
        {
            await _antiforgery.ValidateRequestAsync(context.HttpContext);
        }
        catch (AntiforgeryValidationException)
        {
            context.Result = new BadRequestObjectResult(
                ApiResponse<object>.Erro(ResponseCodes.ERRO, MensagemTokenInvalido));
        }
    }
}
