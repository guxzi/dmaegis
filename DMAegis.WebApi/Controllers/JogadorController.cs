using DMAegis.Services;
using DMAegis.WebApi.ApiModels;
using DMAegis.WebApi.ApiModels.Mesas;
using DMAegis.WebApi.Configuration;
using DMAegis.WebApi.Mappers;
using DMAegis.WebApi.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DMAegis.WebApi.Controllers;

/// <summary>
/// O lado dos jogadores, que entram pelo link de convite (cookie próprio, esquema "Jogador").
/// Só a visão filtrada da mesa ativa: nada do caderno, das anotações ou dos segredos passa por aqui.
/// </summary>
[ApiController]
[Authorize(AuthenticationSchemes = AutenticacaoConfig.EsquemaJogador)]
public class JogadorController : ControllerBase
{
    private readonly IJogadorService _jogadorService;
    private readonly ICanalMesa _canal;
    private readonly IValidator<MoverMinhaFichaRequestApiModel> _validatorMover;
    private readonly IAntiforgery _antiforgery;
    private readonly IRolagemMesaService _rolagemService;
    private readonly IValidator<RolarRequestApiModel> _validatorRolar;

    public JogadorController(IJogadorService jogadorService, ICanalMesa canal,
        IValidator<MoverMinhaFichaRequestApiModel> validatorMover, IAntiforgery antiforgery,
        IRolagemMesaService rolagemService, IValidator<RolarRequestApiModel> validatorRolar)
    {
        _rolagemService = rolagemService;
        _validatorRolar = validatorRolar;
        _jogadorService = jogadorService;
        _canal = canal;
        _validatorMover = validatorMover;
        _antiforgery = antiforgery;
    }

    /// <summary>
    /// Token CSRF do jogador (cookie XSRF-TOKEN-JOGADOR). Emitido aqui, com a identidade do cookie de jogador
    /// (ou anônima, antes de entrar), que é a mesma usada para validar as rotas de jogador.
    /// Pedir de novo depois de entrar.
    /// </summary>
    [HttpGet("api/jogador/csrf")]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public ActionResult<ApiResponse<object>> Csrf()
    {
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
        Response.Cookies.Append(AutenticacaoConfig.NomeCookieCsrfJogador, tokens.RequestToken!, new CookieOptions
        {
            HttpOnly = false,
            SameSite = SameSiteMode.Strict,
            Secure = Request.IsHttps,
            Path = "/",
        });
        return Ok(ApiResponse<object>.Sucesso(null!));
    }

    private static JogadorResponseApiModel ParaResponse(JogadorLogadoServiceModel jogador) => new()
    {
        NomePersonagem = jogador.NomePersonagem,
        NomeCampanha = jogador.NomeCampanha,
    };

    [HttpPost("api/jogador/entrar")]
    [AllowAnonymous]
    [EnableRateLimiting(AutenticacaoConfig.PoliticaLogin)]
    public async Task<ActionResult<ApiResponse<JogadorResponseApiModel>>> Entrar([FromBody] EntrarJogadorRequestApiModel request)
    {
        var jogador = await _jogadorService.EntrarAsync(request.Codigo ?? string.Empty);
        await HttpContext.SignInAsync(AutenticacaoConfig.EsquemaJogador,
            jogador.ParaPrincipalDeJogador(AutenticacaoConfig.EsquemaJogador),
            new AuthenticationProperties { IsPersistent = true });
        return Ok(ApiResponse<JogadorResponseApiModel>.Sucesso(ParaResponse(jogador)));
    }

    [HttpGet("api/jogador/eu")]
    public async Task<ActionResult<ApiResponse<JogadorResponseApiModel>>> Eu()
    {
        var jogador = User.ObterJogador();
        await _jogadorService.ConferirAcessoAsync(jogador);
        return Ok(ApiResponse<JogadorResponseApiModel>.Sucesso(ParaResponse(jogador)));
    }

    [HttpPost("api/jogador/sair")]
    public async Task<ActionResult<ApiResponse<object>>> Sair()
    {
        await HttpContext.SignOutAsync(AutenticacaoConfig.EsquemaJogador);
        return Ok(ApiResponse<object>.Sucesso(null!));
    }

    /// <summary>A mesa ativa, filtrada. Sem mesa ativa, data = null (a tela mostra "aguardando o mestre").</summary>
    [HttpGet("api/jogador/mesa")]
    public async Task<ActionResult<ApiResponse<VisaoJogadorResponseApiModel?>>> Mesa()
    {
        var visao = await _jogadorService.ObterMesaAsync(User.ObterJogador());
        return Ok(ApiResponse<VisaoJogadorResponseApiModel?>.Sucesso(visao?.ParaResponse()));
    }

    [HttpPut("api/jogador/ficha/posicao")]
    public async Task<ActionResult<ApiResponse<object>>> Mover([FromBody] MoverMinhaFichaRequestApiModel request)
    {
        await _validatorMover.ValidarOuLancarAsync(request);
        await _jogadorService.MoverMinhaFichaAsync(User.ObterJogador(), request.Coluna, request.Linha);
        return Ok(ApiResponse<object>.Sucesso(null!));
    }

    [HttpGet("api/jogador/imagens/{id:int}")]
    public async Task<IActionResult> Imagem(int id)
    {
        var (tipo, conteudo) = await _jogadorService.ObterImagemAsync(User.ObterJogador(), id);
        Response.Headers.CacheControl = "private, max-age=31536000, immutable";
        return File(conteudo, tipo);
    }

    [HttpGet("api/jogador/rolagens")]
    public async Task<ActionResult<ApiResponse<List<RolagemResponseApiModel>>>> Rolagens()
    {
        var rolagens = await _rolagemService.ListarDoJogadorAsync(User.ObterJogador());
        return Ok(ApiResponse<List<RolagemResponseApiModel>>.Sucesso(rolagens.Select(r => r.ParaResponse()).ToList()));
    }

    [HttpPost("api/jogador/rolagens")]
    public async Task<ActionResult<ApiResponse<RolagemResponseApiModel>>> Rolar([FromBody] RolarRequestApiModel request)
    {
        await _validatorRolar.ValidarOuLancarAsync(request);
        var rolagem = await _rolagemService.RolarDoJogadorAsync(User.ObterJogador(), request.ParaServiceModel());
        return StatusCode(StatusCodes.Status201Created, ApiResponse<RolagemResponseApiModel>.Sucesso(rolagem.ParaResponse()));
    }

    [HttpGet("api/jogador/eventos")]
    public async Task<IResult> Eventos(CancellationToken cancelamento)
    {
        var jogador = User.ObterJogador();
        await _jogadorService.ConferirAcessoAsync(jogador);
        return EventosMesa.Fluxo(_canal, jogador.IdCampanha, cancelamento);
    }
}
