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

    /// <summary>Segundo esquema: o cookie dos jogadores que entram pelo link de convite. Só abre /api/jogador/*.</summary>
    public const string EsquemaJogador = "Jogador";
    public const string NomeCookieJogador = "dmaegis.jogador";

    /// <summary>
    /// Token CSRF do jogador, num cookie separado do do mestre: o token é ligado à identidade de quem o pede,
    /// e as rotas de jogador validam com a identidade do cookie de jogador.
    /// </summary>
    public const string NomeCookieCsrfJogador = "XSRF-TOKEN-JOGADOR";

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
            })
            .AddCookie(EsquemaJogador, opcoes =>
            {
                opcoes.Cookie.Name = NomeCookieJogador;
                opcoes.Cookie.HttpOnly = true;
                opcoes.Cookie.SameSite = SameSiteMode.Strict;
                opcoes.Cookie.SecurePolicy = ambiente.IsDevelopment()
                    ? CookieSecurePolicy.SameAsRequest
                    : CookieSecurePolicy.Always;
                opcoes.ExpireTimeSpan = TimeSpan.FromDays(30);
                opcoes.SlidingExpiration = true;
                opcoes.Events.OnRedirectToLogin = contexto =>
                    EscreverErroAsync(contexto.HttpContext, StatusCodes.Status401Unauthorized, "Abra o link de convite que o mestre mandou.");
                opcoes.Events.OnRedirectToAccessDenied = contexto =>
                    EscreverErroAsync(contexto.HttpContext, StatusCodes.Status403Forbidden, MensagemAcessoNegado);
            });

        services.AddAuthorization();

        services.AddAntiforgery(opcoes =>
        {
            opcoes.HeaderName = CabecalhoCsrf;
            // Atrás do túnel (https), o cookie interno do antiforgery também só trafega com segurança.
            opcoes.Cookie.SecurePolicy = ambiente.IsDevelopment()
                ? CookieSecurePolicy.SameAsRequest
                : CookieSecurePolicy.Always;
        });

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
