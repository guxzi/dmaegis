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

/// <summary>Rolagens da mesa, do lado do mestre (inclusive as secretas).</summary>
[ApiController]
[Authorize]
public class RolagemController : ControllerBase
{
    private readonly IRolagemMesaService _rolagemService;
    private readonly IValidator<RolarRequestApiModel> _validator;

    public RolagemController(IRolagemMesaService rolagemService, IValidator<RolarRequestApiModel> validator)
    {
        _rolagemService = rolagemService;
        _validator = validator;
    }

    [HttpGet("api/mesas/{idMesa:int}/rolagens")]
    public async Task<ActionResult<ApiResponse<List<RolagemResponseApiModel>>>> Listar(int idMesa)
    {
        var rolagens = await _rolagemService.ListarDoMestreAsync(User.ObterIdUsuario(), idMesa);
        return Ok(ApiResponse<List<RolagemResponseApiModel>>.Sucesso(rolagens.Select(r => r.ParaResponse()).ToList()));
    }

    [HttpPost("api/mesas/{idMesa:int}/rolagens")]
    public async Task<ActionResult<ApiResponse<RolagemResponseApiModel>>> Rolar(int idMesa, [FromBody] RolarRequestApiModel request)
    {
        await _validator.ValidarOuLancarAsync(request);
        var rolagem = await _rolagemService.RolarDoMestreAsync(User.ObterIdUsuario(), idMesa, request.ParaServiceModel());
        return StatusCode(StatusCodes.Status201Created, ApiResponse<RolagemResponseApiModel>.Sucesso(rolagem.ParaResponse()));
    }
}
