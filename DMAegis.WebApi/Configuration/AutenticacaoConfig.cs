using System.Threading.RateLimiting;
using DMAegis.WebApi.ApiModels;
using DMAegis.WebApi.Middlewares;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace DMAegis.WebApi.Configuration;

/// <summary>Cookie de sessão, proteção CSRF e limite de tentativas no login.</summary>
public static class AutenticacaoConfig
{
    public const string NomeCookieSessao = "dmaegis.sessao";
    public const string NomeCookieCsrf = "XSRF-TOKEN";
    public const string CabecalhoCsrf = "X-XSRF-TOKEN";
    public const string PoliticaLogin = "login";

    public const string MensagemNaoAutenticado = "Faça login para continuar.";
    public const string MensagemAcessoNegado = "Você não tem acesso a este recurso.";
    public const string MensagemMuitasTentativas = "Muitas tentativas. Aguarde um minuto e tente de novo.";

    public static void Registrar(IServiceCollection services, IWebHostEnvironment ambiente)
    {
        services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(opcoes =>
            {
                opcoes.Cookie.Name = NomeCookieSessao;
                opcoes.Cookie.HttpOnly = true;
                opcoes.Cookie.SameSite = SameSiteMode.Strict;
                opcoes.Cookie.SecurePolicy = ambiente.IsDevelopment()
                    ? CookieSecurePolicy.SameAsRequest
                    : CookieSecurePolicy.Always;
                opcoes.ExpireTimeSpan = TimeSpan.FromDays(14);
                opcoes.SlidingExpiration = true;

                // API: em vez de redirecionar para uma página de login, responde 401/403 com o envelope.
                opcoes.Events.OnRedirectToLogin = contexto =>
                    EscreverErroAsync(contexto.HttpContext, StatusCodes.Status401Unauthorized, MensagemNaoAutenticado);
                opcoes.Events.OnRedirectToAccessDenied = contexto =>
                    EscreverErroAsync(contexto.HttpContext, StatusCodes.Status403Forbidden, MensagemAcessoNegado);
            });

        services.AddAuthorization();

        services.AddAntiforgery(opcoes => opcoes.HeaderName = CabecalhoCsrf);

        services.AddRateLimiter(opcoes =>
        {
            opcoes.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            opcoes.AddPolicy(PoliticaLogin, contexto =>
                RateLimitPartition.GetFixedWindowLimiter(
                    contexto.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                    }));
            opcoes.OnRejected = (contexto, cancelamento) => new ValueTask(
                EscreverErroAsync(contexto.HttpContext, StatusCodes.Status429TooManyRequests, MensagemMuitasTentativas));
        });

        // Todo POST/PUT/PATCH/DELETE dos controllers exige o token CSRF.
        services.Configure<MvcOptions>(opcoes => opcoes.Filters.Add<ValidarCsrfFilter>());
    }

    private static Task EscreverErroAsync(HttpContext contexto, int status, string mensagem)
    {
        contexto.Response.StatusCode = status;
        return contexto.Response.WriteAsJsonAsync(ApiResponse<object>.Erro(ResponseCodes.ERRO, mensagem));
    }
}
