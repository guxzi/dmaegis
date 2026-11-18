using DMAegis.Services;
using DMAegis.Services.Exceptions;
using DMAegis.WebApi.ApiModels;
using DMAegis.WebApi.ApiModels.Imagens;
using DMAegis.WebApi.Configuration;
using DMAegis.WebApi.Middlewares;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DMAegis.WebApi.Controllers;

/// <summary>Imagens (mapas) anexadas aos registros do caderno.</summary>
[ApiController]
[Authorize]
public class ImagemController : ControllerBase
{
    /// <summary>15 MB de imagem + folga para o resto do formulário multipart.</summary>
    public const int LimiteCorpo = 16 * 1024 * 1024;

    private readonly IImagemService _imagemService;

    public ImagemController(IImagemService imagemService)
    {
        _imagemService = imagemService;
    }

    [HttpGet("api/registros/{idRegistro:int}/imagens")]
    public async Task<ActionResult<ApiResponse<List<ImagemResponseApiModel>>>> Listar(int idRegistro)
    {
        var imagens = await _imagemService.ListarAsync(User.ObterIdUsuario(), idRegistro);
        return Ok(ApiResponse<List<ImagemResponseApiModel>>.Sucesso(imagens.Select(ParaResponse).ToList()));
    }

    /// <summary>
    /// Upload multipart: campo "arquivo" (obrigatório) e "titulo" (opcional; padrão = nome do arquivo sem extensão).
    /// O formulário é lido aqui dentro (sem model binding) para que um corpo grande demais vire o envelope 413.
    /// </summary>
    [HttpPost("api/registros/{idRegistro:int}/imagens")]
    [RequestSizeLimit(LimiteCorpo)]
    [RequestFormLimits(MultipartBodyLengthLimit = LimiteCorpo)]
    [SemLeituraDeFormulario]
    public async Task<ActionResult<ApiResponse<ImagemResponseApiModel>>> Inserir(int idRegistro)
    {
        if (!Request.HasFormContentType)
            throw new BusinessRuleException((BusinessErrors.DadosInvalidos.Code, "Envie a imagem num formulário (multipart)."));

        var formulario = await Request.ReadFormAsync();
        var arquivo = formulario.Files.GetFile("arquivo")
                      ?? throw new BusinessRuleException((BusinessErrors.DadosInvalidos.Code, "Escolha um arquivo de imagem."));

        var titulo = formulario["titulo"].ToString();
        if (string.IsNullOrWhiteSpace(titulo))
            titulo = Path.GetFileNameWithoutExtension(arquivo.FileName);

        using var memoria = new MemoryStream();
        await arquivo.CopyToAsync(memoria);

        var imagem = await _imagemService.InserirAsync(User.ObterIdUsuario(), idRegistro, titulo, memoria.ToArray());
        return StatusCode(StatusCodes.Status201Created, ApiResponse<ImagemResponseApiModel>.Sucesso(ParaResponse(imagem)));
    }

    /// <summary>Os bytes da imagem. Imagens nunca mudam (trocar = excluir e enviar outra), então o cache é longo.</summary>
    [HttpGet("api/imagens/{id:int}")]
    public async Task<IActionResult> Obter(int id)
    {
        var (tipo, conteudo) = await _imagemService.ObterArquivoAsync(User.ObterIdUsuario(), id);
        Response.Headers.CacheControl = "private, max-age=31536000, immutable";
        return File(conteudo, tipo);
    }

    [HttpDelete("api/imagens/{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Excluir(int id)
    {
        await _imagemService.ExcluirAsync(User.ObterIdUsuario(), id);
        return Ok(ApiResponse<object>.Sucesso(null!));
    }

    private static ImagemResponseApiModel ParaResponse(ImagemServiceModel imagem) => new()
    {
        Id = imagem.Id,
        Titulo = imagem.Titulo,
        TipoConteudo = imagem.TipoConteudo,
        Tamanho = imagem.Tamanho,
    };
}
