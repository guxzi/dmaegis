using DMAegis.Services;
using DMAegis.WebApi.ApiModels;
using DMAegis.WebApi.ApiModels.Mesas;
using DMAegis.WebApi.Configuration;
using DMAegis.WebApi.Mappers;
using DMAegis.WebApi.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DMAegis.WebApi.Controllers;

/// <summary>Entregas aos jogadores (handouts), do lado do mestre.</summary>
[ApiController]
[Authorize]
public class EntregaController : ControllerBase
{
    private readonly IEntregaService _entregaService;
    private readonly IValidator<InserirEntregaRequestApiModel> _validator;

    public EntregaController(IEntregaService entregaService, IValidator<InserirEntregaRequestApiModel> validator)
    {
        _entregaService = entregaService;
        _validator = validator;
    }

    [HttpGet("api/campanhas/{idCampanha:int}/entregas")]
    public async Task<ActionResult<ApiResponse<List<EntregaResponseApiModel>>>> Listar(int idCampanha)
    {
        var entregas = await _entregaService.ListarAsync(User.ObterIdUsuario(), idCampanha);
        return Ok(ApiResponse<List<EntregaResponseApiModel>>.Sucesso(entregas.Select(e => e.ParaResponse()).ToList()));
    }

    [HttpPost("api/campanhas/{idCampanha:int}/entregas")]
    public async Task<ActionResult<ApiResponse<EntregaResponseApiModel>>> Inserir(int idCampanha, [FromBody] InserirEntregaRequestApiModel request)
    {
        await _validator.ValidarOuLancarAsync(request);
        var entrega = await _entregaService.InserirAsync(User.ObterIdUsuario(), idCampanha, request.ParaServiceModel());
        return StatusCode(StatusCodes.Status201Created, ApiResponse<EntregaResponseApiModel>.Sucesso(entrega.ParaResponse()));
    }

    /// <summary>Recolhe a entrega: some da tela dos jogadores.</summary>
    [HttpDelete("api/entregas/{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Excluir(int id)
    {
        await _entregaService.ExcluirAsync(User.ObterIdUsuario(), id);
        return Ok(ApiResponse<object>.Sucesso(null!));
    }
}
