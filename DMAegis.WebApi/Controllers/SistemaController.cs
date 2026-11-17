using DMAegis.Services;
using DMAegis.WebApi.ApiModels;
using DMAegis.WebApi.ApiModels.Campanhas;
using DMAegis.WebApi.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DMAegis.WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/sistemas")]
public class SistemaController : ControllerBase
{
    private readonly ICampanhaService _campanhaService;

    public SistemaController(ICampanhaService campanhaService)
    {
        _campanhaService = campanhaService;
    }

    /// <summary>Sistemas disponíveis para uma campanha nova.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<SistemaResponseApiModel>>>> Listar()
    {
        var sistemas = await _campanhaService.ListarSistemasAsync();
        return Ok(ApiResponse<List<SistemaResponseApiModel>>.Sucesso(sistemas.Select(s => s.ParaResponse()).ToList()));
    }
}
