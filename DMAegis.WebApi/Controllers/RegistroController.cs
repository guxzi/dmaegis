using DMAegis.Repository;
using DMAegis.Services;
using DMAegis.WebApi.ApiModels;
using DMAegis.WebApi.ApiModels.Registros;
using DMAegis.WebApi.Configuration;
using DMAegis.WebApi.Mappers;
using DMAegis.WebApi.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DMAegis.WebApi.Controllers;

/// <summary>
/// Registros do caderno. A lista e a criação ficam sob a campanha; o resto usa o id do registro.
/// Registro de campanha de outro usuário responde 404 (código 7).
/// </summary>
[ApiController]
[Authorize]
public class RegistroController : ControllerBase
{
    private readonly IRegistroService _registroService;
    private readonly IValidator<SalvarRegistroRequestApiModel> _validator;

    public RegistroController(IRegistroService registroService, IValidator<SalvarRegistroRequestApiModel> validator)
    {
        _registroService = registroService;
        _validator = validator;
    }

    [HttpGet("api/campanhas/{idCampanha:int}/registros")]
    public async Task<ActionResult<ApiResponse<List<RegistroResumoResponseApiModel>>>> Listar(
        int idCampanha, [FromQuery] int? idTipoRegistro, [FromQuery] int? idTag, [FromQuery] string? texto)
    {
        var filtro = new FiltroRegistro { IdTipoRegistro = idTipoRegistro, IdTag = idTag, Texto = texto };
        var registros = await _registroService.ListarAsync(User.ObterIdUsuario(), idCampanha, filtro);
        return Ok(ApiResponse<List<RegistroResumoResponseApiModel>>.Sucesso(
            registros.Select(r => r.ParaResponse()).ToList()));
    }

    /// <summary>Busca rápida do escudo (Ctrl+K): nome, resumo e tags; até 10 resultados.</summary>
    [HttpGet("api/campanhas/{idCampanha:int}/busca")]
    public async Task<ActionResult<ApiResponse<List<RegistroResumoResponseApiModel>>>> Buscar(
        int idCampanha, [FromQuery] string? q)
    {
        var registros = await _registroService.BuscarAsync(User.ObterIdUsuario(), idCampanha, q);
        return Ok(ApiResponse<List<RegistroResumoResponseApiModel>>.Sucesso(registros.Select(r => r.ParaResponse()).ToList()));
    }

    [HttpPost("api/campanhas/{idCampanha:int}/registros")]
    public async Task<ActionResult<ApiResponse<RegistroResponseApiModel>>> Inserir(
        int idCampanha, [FromBody] SalvarRegistroRequestApiModel request)
    {
        await _validator.ValidarOuLancarAsync(request);
        var registro = await _registroService.InserirAsync(User.ObterIdUsuario(), idCampanha, request.ParaServiceModel());
        return StatusCode(StatusCodes.Status201Created, ApiResponse<RegistroResponseApiModel>.Sucesso(registro.ParaResponse()));
    }

    [HttpGet("api/registros/{id:int}")]
    public async Task<ActionResult<ApiResponse<RegistroResponseApiModel>>> Obter(int id)
    {
        var registro = await _registroService.ObterAsync(User.ObterIdUsuario(), id);
        return Ok(ApiResponse<RegistroResponseApiModel>.Sucesso(registro.ParaResponse()));
    }

    [HttpPut("api/registros/{id:int}")]
    public async Task<ActionResult<ApiResponse<RegistroResponseApiModel>>> Atualizar(
        int id, [FromBody] SalvarRegistroRequestApiModel request)
    {
        await _validator.ValidarOuLancarAsync(request);
        var registro = await _registroService.AtualizarAsync(User.ObterIdUsuario(), id, request.ParaServiceModel());
        return Ok(ApiResponse<RegistroResponseApiModel>.Sucesso(registro.ParaResponse()));
    }

    [HttpDelete("api/registros/{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Excluir(int id)
    {
        await _registroService.ExcluirAsync(User.ObterIdUsuario(), id);
        return Ok(ApiResponse<object>.Sucesso(null!));
    }
}
