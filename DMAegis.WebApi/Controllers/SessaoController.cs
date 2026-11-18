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

/// <summary>Sessões da campanha e o preparo nos 8 passos. Sessão alheia responde 404 (código 10).</summary>
[ApiController]
[Authorize]
public class SessaoController : ControllerBase
{
    private readonly ISessaoService _sessaoService;
    private readonly IValidator<InserirSessaoRequestApiModel> _inserirValidator;
    private readonly IValidator<SalvarSessaoRequestApiModel> _salvarValidator;
    private readonly IValidator<SalvarSegredoRequestApiModel> _segredoValidator;

    public SessaoController(
        ISessaoService sessaoService,
        IValidator<InserirSessaoRequestApiModel> inserirValidator,
        IValidator<SalvarSessaoRequestApiModel> salvarValidator,
        IValidator<SalvarSegredoRequestApiModel> segredoValidator)
    {
        _sessaoService = sessaoService;
        _inserirValidator = inserirValidator;
        _salvarValidator = salvarValidator;
        _segredoValidator = segredoValidator;
    }

    [HttpGet("api/campanhas/{idCampanha:int}/sessoes")]
    public async Task<ActionResult<ApiResponse<List<SessaoResumoResponseApiModel>>>> Listar(int idCampanha)
    {
        var sessoes = await _sessaoService.ListarAsync(User.ObterIdUsuario(), idCampanha);
        return Ok(ApiResponse<List<SessaoResumoResponseApiModel>>.Sucesso(sessoes.Select(s => s.ParaResponse()).ToList()));
    }

    [HttpPost("api/campanhas/{idCampanha:int}/sessoes")]
    public async Task<ActionResult<ApiResponse<SessaoResponseApiModel>>> Inserir(
        int idCampanha, [FromBody] InserirSessaoRequestApiModel? request)
    {
        request ??= new InserirSessaoRequestApiModel();
        await _inserirValidator.ValidarOuLancarAsync(request);
        var sessao = await _sessaoService.InserirAsync(User.ObterIdUsuario(), idCampanha, request.ParaServiceModel());
        return StatusCode(StatusCodes.Status201Created, ApiResponse<SessaoResponseApiModel>.Sucesso(sessao.ParaResponse()));
    }

    [HttpGet("api/sessoes/{id:int}")]
    public async Task<ActionResult<ApiResponse<SessaoResponseApiModel>>> Obter(int id)
    {
        var sessao = await _sessaoService.ObterAsync(User.ObterIdUsuario(), id);
        return Ok(ApiResponse<SessaoResponseApiModel>.Sucesso(sessao.ParaResponse()));
    }

    [HttpPut("api/sessoes/{id:int}")]
    public async Task<ActionResult<ApiResponse<SessaoResponseApiModel>>> Atualizar(
        int id, [FromBody] SalvarSessaoRequestApiModel request)
    {
        await _salvarValidator.ValidarOuLancarAsync(request);
        var sessao = await _sessaoService.AtualizarAsync(User.ObterIdUsuario(), id, request.ParaServiceModel());
        return Ok(ApiResponse<SessaoResponseApiModel>.Sucesso(sessao.ParaResponse()));
    }

    [HttpDelete("api/sessoes/{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Excluir(int id)
    {
        await _sessaoService.ExcluirAsync(User.ObterIdUsuario(), id);
        return Ok(ApiResponse<object>.Sucesso(null!));
    }

    /// <summary>Cria um segredo na campanha da sessão, marcado como criado nela.</summary>
    [HttpPost("api/sessoes/{id:int}/segredos")]
    public async Task<ActionResult<ApiResponse<SegredoResponseApiModel>>> InserirSegredo(
        int id, [FromBody] SalvarSegredoRequestApiModel request)
    {
        await _segredoValidator.ValidarOuLancarAsync(request);
        var segredo = await _sessaoService.InserirSegredoAsync(User.ObterIdUsuario(), id, request.Texto);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<SegredoResponseApiModel>.Sucesso(segredo.ParaResponse()));
    }
}
