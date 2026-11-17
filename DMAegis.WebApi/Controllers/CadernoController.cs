using DMAegis.Services;
using DMAegis.WebApi.ApiModels;
using DMAegis.WebApi.ApiModels.Registros;
using DMAegis.WebApi.Configuration;
using DMAegis.WebApi.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DMAegis.WebApi.Controllers;

/// <summary>Dados de apoio do caderno: tipos de registro, modelos de atributo e tags da campanha.</summary>
[ApiController]
[Authorize]
public class CadernoController : ControllerBase
{
    private readonly IRegistroService _registroService;

    public CadernoController(IRegistroService registroService)
    {
        _registroService = registroService;
    }

    [HttpGet("api/tipos-registro")]
    public async Task<ActionResult<ApiResponse<List<TipoRegistroResponseApiModel>>>> ListarTipos()
    {
        var tipos = await _registroService.ListarTiposAsync();
        return Ok(ApiResponse<List<TipoRegistroResponseApiModel>>.Sucesso(tipos.Select(t => t.ParaResponse()).ToList()));
    }

    /// <summary>Campos que o formulário mostra para cada tipo (ex.: "CA" na Criatura do D&amp;D).</summary>
    [HttpGet("api/sistemas/{idSistema:int}/modelos-atributo")]
    public async Task<ActionResult<ApiResponse<List<ModeloAtributoResponseApiModel>>>> ListarModelos(
        int idSistema, [FromQuery] int? idTipoRegistro)
    {
        var modelos = await _registroService.ListarModelosAsync(idSistema, idTipoRegistro);
        return Ok(ApiResponse<List<ModeloAtributoResponseApiModel>>.Sucesso(modelos.Select(m => m.ParaResponse()).ToList()));
    }

    [HttpGet("api/campanhas/{idCampanha:int}/tags")]
    public async Task<ActionResult<ApiResponse<List<TagResponseApiModel>>>> ListarTags(int idCampanha)
    {
        var tags = await _registroService.ListarTagsAsync(User.ObterIdUsuario(), idCampanha);
        return Ok(ApiResponse<List<TagResponseApiModel>>.Sucesso(tags.Select(t => t.ParaResponse()).ToList()));
    }
}
