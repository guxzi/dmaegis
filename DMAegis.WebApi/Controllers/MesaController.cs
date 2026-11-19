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

/// <summary>Mesa virtual: mapas da campanha, fichas e combate. O Service confere o dono da campanha.</summary>
[ApiController]
[Authorize]
public class MesaController : ControllerBase
{
    private readonly IMesaService _mesaService;
    private readonly IValidator<SalvarMesaRequestApiModel> _validatorMesa;
    private readonly IValidator<InserirParticipantesRequestApiModel> _validatorInserir;
    private readonly IValidator<SalvarParticipanteRequestApiModel> _validatorParticipante;
    private readonly IValidator<MoverParticipanteRequestApiModel> _validatorMover;

    public MesaController(
        IMesaService mesaService,
        IValidator<SalvarMesaRequestApiModel> validatorMesa,
        IValidator<InserirParticipantesRequestApiModel> validatorInserir,
        IValidator<SalvarParticipanteRequestApiModel> validatorParticipante,
        IValidator<MoverParticipanteRequestApiModel> validatorMover)
    {
        _mesaService = mesaService;
        _validatorMesa = validatorMesa;
        _validatorInserir = validatorInserir;
        _validatorParticipante = validatorParticipante;
        _validatorMover = validatorMover;
    }

    private ActionResult<ApiResponse<MesaResponseApiModel>> Mesa(MesaServiceModel mesa) =>
        Ok(ApiResponse<MesaResponseApiModel>.Sucesso(mesa.ParaResponse()));

    [HttpGet("api/campanhas/{idCampanha:int}/mesas")]
    public async Task<ActionResult<ApiResponse<List<MesaResumoResponseApiModel>>>> Listar(int idCampanha)
    {
        var mesas = await _mesaService.ListarAsync(User.ObterIdUsuario(), idCampanha);
        return Ok(ApiResponse<List<MesaResumoResponseApiModel>>.Sucesso(mesas.Select(m => m.ParaResponse()).ToList()));
    }

    [HttpPost("api/campanhas/{idCampanha:int}/mesas")]
    public async Task<ActionResult<ApiResponse<MesaResponseApiModel>>> Inserir(
        int idCampanha, [FromBody] SalvarMesaRequestApiModel request)
    {
        await _validatorMesa.ValidarOuLancarAsync(request);
        var mesa = await _mesaService.InserirAsync(User.ObterIdUsuario(), idCampanha, request.ParaServiceModel());
        return StatusCode(StatusCodes.Status201Created, ApiResponse<MesaResponseApiModel>.Sucesso(mesa.ParaResponse()));
    }

    [HttpGet("api/mesas/{id:int}")]
    public async Task<ActionResult<ApiResponse<MesaResponseApiModel>>> Obter(int id) =>
        Mesa(await _mesaService.ObterAsync(User.ObterIdUsuario(), id));

    [HttpPut("api/mesas/{id:int}")]
    public async Task<ActionResult<ApiResponse<MesaResponseApiModel>>> Atualizar(int id, [FromBody] SalvarMesaRequestApiModel request)
    {
        await _validatorMesa.ValidarOuLancarAsync(request);
        return Mesa(await _mesaService.AtualizarAsync(User.ObterIdUsuario(), id, request.ParaServiceModel()));
    }

    [HttpDelete("api/mesas/{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Excluir(int id)
    {
        await _mesaService.ExcluirAsync(User.ObterIdUsuario(), id);
        return Ok(ApiResponse<object>.Sucesso(null!));
    }

    [HttpPut("api/mesas/{id:int}/ativa")]
    public async Task<ActionResult<ApiResponse<MesaResponseApiModel>>> Ativar(int id) =>
        Mesa(await _mesaService.AtivarAsync(User.ObterIdUsuario(), id));

    [HttpPost("api/mesas/{id:int}/participantes")]
    public async Task<ActionResult<ApiResponse<MesaResponseApiModel>>> InserirParticipantes(
        int id, [FromBody] InserirParticipantesRequestApiModel request)
    {
        await _validatorInserir.ValidarOuLancarAsync(request);
        var mesa = await _mesaService.InserirParticipantesAsync(User.ObterIdUsuario(), id, request.ParaServiceModel());
        return StatusCode(StatusCodes.Status201Created, ApiResponse<MesaResponseApiModel>.Sucesso(mesa.ParaResponse()));
    }

    [HttpPut("api/participantes/{id:int}")]
    public async Task<ActionResult<ApiResponse<MesaResponseApiModel>>> AtualizarParticipante(
        int id, [FromBody] SalvarParticipanteRequestApiModel request)
    {
        await _validatorParticipante.ValidarOuLancarAsync(request);
        return Mesa(await _mesaService.AtualizarParticipanteAsync(User.ObterIdUsuario(), id, request.ParaServiceModel()));
    }

    /// <summary>Só a posição, para o arrasto ser leve. Responde sem a mesa.</summary>
    [HttpPut("api/participantes/{id:int}/posicao")]
    public async Task<ActionResult<ApiResponse<object>>> Mover(int id, [FromBody] MoverParticipanteRequestApiModel request)
    {
        await _validatorMover.ValidarOuLancarAsync(request);
        await _mesaService.MoverParticipanteAsync(User.ObterIdUsuario(), id, request.Coluna, request.Linha);
        return Ok(ApiResponse<object>.Sucesso(null!));
    }

    [HttpDelete("api/participantes/{id:int}")]
    public async Task<ActionResult<ApiResponse<MesaResponseApiModel>>> ExcluirParticipante(int id) =>
        Mesa(await _mesaService.ExcluirParticipanteAsync(User.ObterIdUsuario(), id));

    [HttpPost("api/mesas/{id:int}/combate")]
    public async Task<ActionResult<ApiResponse<MesaResponseApiModel>>> IniciarCombate(
        int id, [FromBody] IniciarCombateRequestApiModel? request) =>
        Mesa(await _mesaService.IniciarCombateAsync(User.ObterIdUsuario(), id, request.ParaServiceModel()));

    [HttpPost("api/mesas/{id:int}/combate/proximo")]
    public async Task<ActionResult<ApiResponse<MesaResponseApiModel>>> ProximoTurno(int id) =>
        Mesa(await _mesaService.ProximoTurnoAsync(User.ObterIdUsuario(), id));

    [HttpDelete("api/mesas/{id:int}/combate")]
    public async Task<ActionResult<ApiResponse<MesaResponseApiModel>>> EncerrarCombate(int id) =>
        Mesa(await _mesaService.EncerrarCombateAsync(User.ObterIdUsuario(), id));

    /// <summary>O estado filtrado que os jogadores veem (usado pela janela de projeção).</summary>
    [HttpGet("api/mesas/{id:int}/visao-jogador")]
    public async Task<ActionResult<ApiResponse<VisaoJogadorResponseApiModel>>> VisaoJogador(int id)
    {
        var visao = await _mesaService.ObterVisaoJogadorAsync(User.ObterIdUsuario(), id);
        return Ok(ApiResponse<VisaoJogadorResponseApiModel>.Sucesso(visao.ParaResponse()));
    }
}
