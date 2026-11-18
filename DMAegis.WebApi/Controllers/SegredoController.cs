using DMAegis.Services;
using DMAegis.WebApi.ApiModels;
using DMAegis.WebApi.ApiModels.Sessoes;
using DMAegis.WebApi.Configuration;
using DMAegis.WebApi.Mappers;
using DMAegis.WebApi.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DMAegis.WebApi.Controllers;

/// <summary>Ações imediatas sobre um segredo. Segredo alheio responde 404 (código 11).</summary>
[ApiController]
[Authorize]
[Route("api/segredos/{id:int}")]
public class SegredoController : ControllerBase
{
    private readonly ISessaoService _sessaoService;
    private readonly IValidator<SalvarSegredoRequestApiModel> _validator;

    public SegredoController(ISessaoService sessaoService, IValidator<SalvarSegredoRequestApiModel> validator)
    {
        _sessaoService = sessaoService;
        _validator = validator;
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse<SegredoResponseApiModel>>> Atualizar(
        int id, [FromBody] SalvarSegredoRequestApiModel request)
    {
        await _validator.ValidarOuLancarAsync(request);
        var segredo = await _sessaoService.AtualizarSegredoAsync(User.ObterIdUsuario(), id, request.Texto);
        return Ok(ApiResponse<SegredoResponseApiModel>.Sucesso(segredo.ParaResponse()));
    }

    /// <summary>Marca o segredo como revelado na sessão informada (da mesma campanha).</summary>
    [HttpPost("revelar")]
    public async Task<ActionResult<ApiResponse<SegredoResponseApiModel>>> Revelar(
        int id, [FromBody] RevelarSegredoRequestApiModel request)
    {
        var segredo = await _sessaoService.RevelarSegredoAsync(User.ObterIdUsuario(), id, request.IdSessao);
        return Ok(ApiResponse<SegredoResponseApiModel>.Sucesso(segredo.ParaResponse()));
    }

    /// <summary>Desfaz a revelação: o segredo volta a ficar pendente.</summary>
    [HttpPost("ocultar")]
    public async Task<ActionResult<ApiResponse<SegredoResponseApiModel>>> Ocultar(int id)
    {
        var segredo = await _sessaoService.OcultarSegredoAsync(User.ObterIdUsuario(), id);
        return Ok(ApiResponse<SegredoResponseApiModel>.Sucesso(segredo.ParaResponse()));
    }

    [HttpDelete]
    public async Task<ActionResult<ApiResponse<object>>> Excluir(int id)
    {
        await _sessaoService.ExcluirSegredoAsync(User.ObterIdUsuario(), id);
        return Ok(ApiResponse<object>.Sucesso(null!));
    }
}
