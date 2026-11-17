using DMAegis.Services;
using DMAegis.WebApi.ApiModels;
using DMAegis.WebApi.ApiModels.Campanhas;
using DMAegis.WebApi.Configuration;
using DMAegis.WebApi.Mappers;
using DMAegis.WebApi.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DMAegis.WebApi.Controllers;

/// <summary>
/// Campanhas do usuário logado. O Service confere o dono em toda operação:
/// campanha de outro usuário responde 404, como se não existisse.
/// </summary>
[ApiController]
[Authorize]
[Route("api/campanhas")]
public class CampanhaController : ControllerBase
{
    private readonly ICampanhaService _campanhaService;
    private readonly IValidator<InserirCampanhaRequestApiModel> _inserirValidator;
    private readonly IValidator<AtualizarCampanhaRequestApiModel> _atualizarValidator;

    public CampanhaController(
        ICampanhaService campanhaService,
        IValidator<InserirCampanhaRequestApiModel> inserirValidator,
        IValidator<AtualizarCampanhaRequestApiModel> atualizarValidator)
    {
        _campanhaService = campanhaService;
        _inserirValidator = inserirValidator;
        _atualizarValidator = atualizarValidator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CampanhaResponseApiModel>>>> Listar()
    {
        var campanhas = await _campanhaService.ListarAsync(User.ObterIdUsuario());
        return Ok(ApiResponse<List<CampanhaResponseApiModel>>.Sucesso(campanhas.Select(c => c.ParaResponse()).ToList()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<CampanhaResponseApiModel>>> Obter(int id)
    {
        var campanha = await _campanhaService.ObterAsync(User.ObterIdUsuario(), id);
        return Ok(ApiResponse<CampanhaResponseApiModel>.Sucesso(campanha.ParaResponse()));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CampanhaResponseApiModel>>> Inserir(
        [FromBody] InserirCampanhaRequestApiModel request)
    {
        await _inserirValidator.ValidarOuLancarAsync(request);
        var campanha = await _campanhaService.InserirAsync(User.ObterIdUsuario(), request.ParaServiceModel());
        return StatusCode(StatusCodes.Status201Created, ApiResponse<CampanhaResponseApiModel>.Sucesso(campanha.ParaResponse()));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<CampanhaResponseApiModel>>> Atualizar(
        int id, [FromBody] AtualizarCampanhaRequestApiModel request)
    {
        await _atualizarValidator.ValidarOuLancarAsync(request);
        var campanha = await _campanhaService.AtualizarAsync(User.ObterIdUsuario(), id, request.ParaServiceModel());
        return Ok(ApiResponse<CampanhaResponseApiModel>.Sucesso(campanha.ParaResponse()));
    }

    [HttpPost("{id:int}/arquivar")]
    public Task<ActionResult<ApiResponse<CampanhaResponseApiModel>>> Arquivar(int id) => DefinirArquivada(id, true);

    [HttpPost("{id:int}/desarquivar")]
    public Task<ActionResult<ApiResponse<CampanhaResponseApiModel>>> Desarquivar(int id) => DefinirArquivada(id, false);

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Excluir(int id)
    {
        await _campanhaService.ExcluirAsync(User.ObterIdUsuario(), id);
        return Ok(ApiResponse<object>.Sucesso(null!));
    }

    private async Task<ActionResult<ApiResponse<CampanhaResponseApiModel>>> DefinirArquivada(int id, bool arquivada)
    {
        var campanha = await _campanhaService.DefinirArquivadaAsync(User.ObterIdUsuario(), id, arquivada);
        return Ok(ApiResponse<CampanhaResponseApiModel>.Sucesso(campanha.ParaResponse()));
    }
}
