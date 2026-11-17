using System.Security.Claims;
using DMAegis.Services;
using DMAegis.WebApi.ApiModels;
using DMAegis.WebApi.ApiModels.Auth;
using DMAegis.WebApi.Configuration;
using DMAegis.WebApi.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DMAegis.WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;
    private readonly IAntiforgery _antiforgery;
    private readonly IValidator<CadastrarUsuarioRequestApiModel> _cadastroValidator;
    private readonly IValidator<LoginRequestApiModel> _loginValidator;

    public AuthController(
        IUsuarioService usuarioService,
        IAntiforgery antiforgery,
        IValidator<CadastrarUsuarioRequestApiModel> cadastroValidator,
        IValidator<LoginRequestApiModel> loginValidator)
    {
        _usuarioService = usuarioService;
        _antiforgery = antiforgery;
        _cadastroValidator = cadastroValidator;
        _loginValidator = loginValidator;
    }

    /// <summary>
    /// Grava o cookie XSRF-TOKEN (legível pelo JS). O front devolve o valor no cabeçalho X-XSRF-TOKEN.
    /// Precisa ser chamado de novo depois de entrar ou sair, porque o token é ligado ao usuário.
    /// </summary>
    [HttpGet("csrf")]
    [IgnoreAntiforgeryToken]
    public ActionResult<ApiResponse<object>> Csrf()
    {
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
        Response.Cookies.Append(AutenticacaoConfig.NomeCookieCsrf, tokens.RequestToken!, new CookieOptions
        {
            HttpOnly = false,
            SameSite = SameSiteMode.Strict,
            Secure = Request.IsHttps,
            Path = "/",
        });
        return Ok(ApiResponse<object>.Sucesso(null!));
    }

    [HttpPost("cadastro")]
    public async Task<ActionResult<ApiResponse<UsuarioResponseApiModel>>> Cadastrar(
        [FromBody] CadastrarUsuarioRequestApiModel request)
    {
        await _cadastroValidator.ValidarOuLancarAsync(request);

        var usuario = await _usuarioService.CadastrarAsync(new CadastrarUsuarioServiceModel
        {
            Nome = request.Nome,
            Email = request.Email,
            Senha = request.Senha,
        });
        await EntrarComCookieAsync(usuario);

        return StatusCode(StatusCodes.Status201Created, ApiResponse<UsuarioResponseApiModel>.Sucesso(ParaResponse(usuario)));
    }

    [HttpPost("login")]
    [EnableRateLimiting(AutenticacaoConfig.PoliticaLogin)]
    public async Task<ActionResult<ApiResponse<UsuarioResponseApiModel>>> Login([FromBody] LoginRequestApiModel request)
    {
        await _loginValidator.ValidarOuLancarAsync(request);

        var usuario = await _usuarioService.AutenticarAsync(request.Email, request.Senha);
        await EntrarComCookieAsync(usuario);

        return Ok(ApiResponse<UsuarioResponseApiModel>.Sucesso(ParaResponse(usuario)));
    }

    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<object>>> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(ApiResponse<object>.Sucesso(null!));
    }

    [HttpGet("eu")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UsuarioResponseApiModel>>> Eu()
    {
        var usuario = await _usuarioService.ObterAsync(User.ObterIdUsuario());

        if (usuario is null)
        {
            // O cookie ainda é válido, mas a conta não existe mais: encerra a sessão.
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Unauthorized(ApiResponse<UsuarioResponseApiModel>.Erro(
                ResponseCodes.ERRO, AutenticacaoConfig.MensagemNaoAutenticado));
        }

        return Ok(ApiResponse<UsuarioResponseApiModel>.Sucesso(ParaResponse(usuario)));
    }

    private Task EntrarComCookieAsync(UsuarioServiceModel usuario)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.Email, usuario.Email),
        };
        var identidade = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identidade),
            new AuthenticationProperties { IsPersistent = true });
    }

    private static UsuarioResponseApiModel ParaResponse(UsuarioServiceModel usuario) => new()
    {
        Id = usuario.Id,
        Nome = usuario.Nome,
        Email = usuario.Email,
    };
}
