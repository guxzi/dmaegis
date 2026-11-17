using DMAegis.Services;
using DMAegis.WebApi.ApiModels;
using Microsoft.AspNetCore.Mvc;

namespace DMAegis.WebApi.Controllers;

[ApiController]
[Route("api/saude")]
public class SaudeController : ControllerBase
{
    private readonly ISaudeService _saudeService;

    public SaudeController(ISaudeService saudeService)
    {
        _saudeService = saudeService;
    }

    /// <summary>Diz se a API está no ar e se o banco responde.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<SaudeServiceModel>>> Obter()
    {
        var saude = await _saudeService.VerificarAsync();
        return Ok(ApiResponse<SaudeServiceModel>.Sucesso(saude));
    }
}
