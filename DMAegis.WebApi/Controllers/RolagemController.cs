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
    private readonly IResumoMesaService _resumoService;
    private readonly ISessaoService _sessaoService;

    public RolagemController(IRolagemMesaService rolagemService, IValidator<RolarRequestApiModel> validator,
        IResumoMesaService resumoService, ISessaoService sessaoService)
    {
        _resumoService = resumoService;
        _sessaoService = sessaoService;
        _rolagemService = rolagemService;
        _validator = validator;
    }

    [HttpGet("api/mesas/{idMesa:int}/rolagens")]
    public async Task<ActionResult<ApiResponse<List<RolagemResponseApiModel>>>> Listar(int idMesa)
    {
        var rolagens = await _rolagemService.ListarDoMestreAsync(User.ObterIdUsuario(), idMesa);
        return Ok(ApiResponse<List<RolagemResponseApiModel>>.Sucesso(rolagens.Select(r => r.ParaResponse()).ToList()));
    }

    /// <summary>Rascunho do resumo da sessão: combates, quedas, críticos e entregas das últimas horas.</summary>
    [HttpGet("api/mesas/{idMesa:int}/resumo")]
    public async Task<ActionResult<ApiResponse<ResumoMesaResponseApiModel>>> Resumo(int idMesa, [FromQuery] int horas = 12)
    {
        var texto = await _resumoService.GerarAsync(User.ObterIdUsuario(), idMesa, horas);
        return Ok(ApiResponse<ResumoMesaResponseApiModel>.Sucesso(new ResumoMesaResponseApiModel { Texto = texto }));
    }

    /// <summary>Acrescenta o texto (o resumo revisado pelo mestre) ao fim do resumo da sessão.</summary>
    [HttpPost("api/sessoes/{idSessao:int}/resumo")]
    public async Task<ActionResult<ApiResponse<object>>> AnexarResumo(int idSessao, [FromBody] AnexarResumoRequestApiModel request)
    {
        if ((request.Texto ?? string.Empty).Length > 20000)
            throw new DMAegis.Services.Exceptions.BusinessRuleException((DMAegis.Services.Exceptions.BusinessErrors.DadosInvalidos.Code, "O resumo pode ter até 20 000 caracteres."));
        await _sessaoService.AnexarAoResumoAsync(User.ObterIdUsuario(), idSessao, request.Texto ?? string.Empty);
        return Ok(ApiResponse<object>.Sucesso(null!));
    }

    [HttpPost("api/mesas/{idMesa:int}/rolagens")]
    public async Task<ActionResult<ApiResponse<RolagemResponseApiModel>>> Rolar(int idMesa, [FromBody] RolarRequestApiModel request)
    {
        await _validator.ValidarOuLancarAsync(request);
        var rolagem = await _rolagemService.RolarDoMestreAsync(User.ObterIdUsuario(), idMesa, request.ParaServiceModel());
        return StatusCode(StatusCodes.Status201Created, ApiResponse<RolagemResponseApiModel>.Sucesso(rolagem.ParaResponse()));
    }
}
