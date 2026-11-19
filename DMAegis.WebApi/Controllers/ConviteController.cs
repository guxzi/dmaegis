using DMAegis.Services;
using DMAegis.WebApi.ApiModels;
using DMAegis.WebApi.ApiModels.Mesas;
using DMAegis.WebApi.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DMAegis.WebApi.Controllers;

/// <summary>O lado do mestre: links de convite dos PJs e os avisos em tempo real da mesa.</summary>
[ApiController]
[Authorize]
public class ConviteController : ControllerBase
{
    private readonly IJogadorService _jogadorService;
    private readonly ICampanhaService _campanhaService;
    private readonly ICanalMesa _canal;

    public ConviteController(IJogadorService jogadorService, ICampanhaService campanhaService, ICanalMesa canal)
    {
        _jogadorService = jogadorService;
        _campanhaService = campanhaService;
        _canal = canal;
    }

    [HttpGet("api/campanhas/{idCampanha:int}/convites")]
    public async Task<ActionResult<ApiResponse<List<ConviteResponseApiModel>>>> Listar(int idCampanha)
    {
        var convites = await _jogadorService.ListarConvitesAsync(User.ObterIdUsuario(), idCampanha);
        return Ok(ApiResponse<List<ConviteResponseApiModel>>.Sucesso(convites.Select(c => new ConviteResponseApiModel
        {
            IdRegistro = c.IdRegistro,
            NomePersonagem = c.NomePersonagem,
            TemConvite = c.TemConvite,
            DataUltimoAcesso = c.DataUltimoAcesso,
        }).ToList()));
    }

    /// <summary>Gera ou troca o link do PJ. O link antigo para de valer na hora.</summary>
    [HttpPost("api/registros/{idRegistro:int}/convite")]
    public async Task<ActionResult<ApiResponse<CodigoConviteResponseApiModel>>> Gerar(int idRegistro)
    {
        var codigo = await _jogadorService.GerarConviteAsync(User.ObterIdUsuario(), idRegistro);
        return Ok(ApiResponse<CodigoConviteResponseApiModel>.Sucesso(new CodigoConviteResponseApiModel { IdRegistro = idRegistro, Codigo = codigo }));
    }

    [HttpDelete("api/registros/{idRegistro:int}/convite")]
    public async Task<ActionResult<ApiResponse<object>>> Revogar(int idRegistro)
    {
        await _jogadorService.RevogarConviteAsync(User.ObterIdUsuario(), idRegistro);
        return Ok(ApiResponse<object>.Sucesso(null!));
    }

    /// <summary>Avisos da mesa para a tela do mestre (por exemplo, quando um jogador move a ficha).</summary>
    [HttpGet("api/campanhas/{idCampanha:int}/mesa/eventos")]
    public async Task<IResult> Eventos(int idCampanha, CancellationToken cancelamento)
    {
        await _campanhaService.ObterDoUsuarioOuLancarAsync(User.ObterIdUsuario(), idCampanha);
        return EventosMesa.Fluxo(_canal, idCampanha, cancelamento);
    }
}
