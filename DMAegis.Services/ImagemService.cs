using DMAegis.Model;
using DMAegis.Repository;
using DMAegis.Services.Exceptions;

namespace DMAegis.Services;

public class ImagemServiceModel
{
    public int Id { get; set; }
    public int IdRegistro { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string TipoConteudo { get; set; } = string.Empty;
    public int Tamanho { get; set; }
}

public interface IImagemService
{
    Task<IEnumerable<ImagemServiceModel>> ListarAsync(int idUsuario, int idRegistro);

    /// <summary>Anexa uma imagem ao registro. O tipo é detectado pelos bytes, nunca pelo nome do arquivo.</summary>
    Task<ImagemServiceModel> InserirAsync(int idUsuario, int idRegistro, string titulo, byte[] conteudo);

    /// <summary>Bytes e tipo da imagem. Imagem inexistente ou de registro alheio → ImagemNaoEncontrada.</summary>
    Task<(string TipoConteudo, byte[] Conteudo)> ObterArquivoAsync(int idUsuario, int id);

    Task ExcluirAsync(int idUsuario, int id);
}

public class ImagemService : IImagemService
{
    public const int TamanhoMaximo = 15 * 1024 * 1024;
    public const int MaximoPorRegistro = 30;
    public const int TamanhoMaximoTitulo = 150;

    private readonly IImagemRegistroRepository _imagemRepository;
    private readonly IRegistroService _registroService;

    public ImagemService(IImagemRegistroRepository imagemRepository, IRegistroService registroService)
    {
        _imagemRepository = imagemRepository;
        _registroService = registroService;
    }

    /// <summary>Tipo da imagem pela assinatura dos primeiros bytes; null se não for JPEG, PNG, GIF ou WebP.</summary>
    public static string? DetectarTipo(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
            return "image/jpeg";
        if (bytes.Length >= 8 && bytes[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
            return "image/png";
        if (bytes.Length >= 6 && (bytes[..6].SequenceEqual("GIF87a"u8) || bytes[..6].SequenceEqual("GIF89a"u8)))
            return "image/gif";
        if (bytes.Length >= 12 && bytes[..4].SequenceEqual("RIFF"u8) && bytes[8..12].SequenceEqual("WEBP"u8))
            return "image/webp";
        return null;
    }

    public async Task<IEnumerable<ImagemServiceModel>> ListarAsync(int idUsuario, int idRegistro)
    {
        await _registroService.ObterAsync(idUsuario, idRegistro);
        return (await _imagemRepository.ListarPorRegistroAsync(idRegistro)).Select(ParaServiceModel).ToList();
    }

    public async Task<ImagemServiceModel> InserirAsync(int idUsuario, int idRegistro, string titulo, byte[] conteudo)
    {
        await _registroService.ObterAsync(idUsuario, idRegistro);

        var tituloLimpo = (titulo ?? string.Empty).Trim();
        if (tituloLimpo.Length == 0)
            throw new BusinessRuleException((BusinessErrors.DadosInvalidos.Code, "Informe o título da imagem."));
        if (tituloLimpo.Length > TamanhoMaximoTitulo)
            throw new BusinessRuleException((BusinessErrors.DadosInvalidos.Code, $"O título pode ter até {TamanhoMaximoTitulo} caracteres."));
        if (conteudo.Length > TamanhoMaximo)
            throw new BusinessRuleException(BusinessErrors.ImagemGrandeDemais);

        var tipo = DetectarTipo(conteudo) ?? throw new BusinessRuleException(BusinessErrors.ImagemInvalida);

        var quantidade = await _imagemRepository.ContarPorRegistroAsync(idRegistro);
        if (quantidade >= MaximoPorRegistro)
            throw new BusinessRuleException(BusinessErrors.LimiteImagens);

        var imagem = new ImagemRegistro
        {
            IdRegistro = idRegistro,
            Titulo = tituloLimpo,
            TipoConteudo = tipo,
            Tamanho = conteudo.Length,
            Ordem = quantidade + 1,
            DataCriacao = DateTime.Now,
            Conteudo = conteudo,
        };
        imagem.Id = await _imagemRepository.InserirAsync(imagem);
        return ParaServiceModel(imagem);
    }

    public async Task<(string TipoConteudo, byte[] Conteudo)> ObterArquivoAsync(int idUsuario, int id)
    {
        await ObterDoUsuarioOuLancarAsync(idUsuario, id);
        return await _imagemRepository.ObterConteudoAsync(id)
               ?? throw new BusinessRuleException(BusinessErrors.ImagemNaoEncontrada);
    }

    public async Task ExcluirAsync(int idUsuario, int id)
    {
        await ObterDoUsuarioOuLancarAsync(idUsuario, id);
        await _imagemRepository.ExcluirAsync(id);
    }

    /// <summary>Imagem inexistente ou de registro alheio → ImagemNaoEncontrada (404).</summary>
    private async Task<ImagemRegistro> ObterDoUsuarioOuLancarAsync(int idUsuario, int id)
    {
        var imagem = await _imagemRepository.ObterMetadadosAsync(id)
                     ?? throw new BusinessRuleException(BusinessErrors.ImagemNaoEncontrada);
        try
        {
            await _registroService.ObterAsync(idUsuario, imagem.IdRegistro);
        }
        catch (BusinessRuleException e) when (e.Code == BusinessErrors.RegistroNaoEncontrado.Code)
        {
            throw new BusinessRuleException(BusinessErrors.ImagemNaoEncontrada);
        }
        return imagem;
    }

    private static ImagemServiceModel ParaServiceModel(ImagemRegistro imagem) => new()
    {
        Id = imagem.Id,
        IdRegistro = imagem.IdRegistro,
        Titulo = imagem.Titulo,
        TipoConteudo = imagem.TipoConteudo,
        Tamanho = imagem.Tamanho,
    };
}
