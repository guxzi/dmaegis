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

/// <summary>Anotações com horário, feitas no escudo durante a sessão.</summary>
[ApiController]
[Authorize]
public class AnotacaoController : ControllerBase
{
    private readonly ISessaoService _sessaoService;
    private readonly IValidator<SalvarAnotacaoRequestApiModel> _validator;

    public AnotacaoController(ISessaoService sessaoService, IValidator<SalvarAnotacaoRequestApiModel> validator)
    {
        _sessaoService = sessaoService;
        _validator = validator;
    }

    [HttpGet("api/sessoes/{idSessao:int}/anotacoes")]
    public async Task<ActionResult<ApiResponse<List<AnotacaoResponseApiModel>>>> Listar(int idSessao)
    {
        var anotacoes = await _sessaoService.ListarAnotacoesAsync(User.ObterIdUsuario(), idSessao);
        return Ok(ApiResponse<List<AnotacaoResponseApiModel>>.Sucesso(anotacoes.Select(a => a.ParaResponse()).ToList()));
    }

    [HttpPost("api/sessoes/{idSessao:int}/anotacoes")]
    public async Task<ActionResult<ApiResponse<AnotacaoResponseApiModel>>> Inserir(
        int idSessao, [FromBody] SalvarAnotacaoRequestApiModel request)
    {
        await _validator.ValidarOuLancarAsync(request);
        var anotacao = await _sessaoService.InserirAnotacaoAsync(User.ObterIdUsuario(), idSessao, request.Texto);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<AnotacaoResponseApiModel>.Sucesso(anotacao.ParaResponse()));
    }

    [HttpDelete("api/anotacoes/{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Excluir(int id)
    {
        await _sessaoService.ExcluirAnotacaoAsync(User.ObterIdUsuario(), id);
        return Ok(ApiResponse<object>.Sucesso(null!));
    }
}
