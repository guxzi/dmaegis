namespace DMAegis.WebApi.ApiModels.Imagens;

public class ImagemResponseApiModel
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string TipoConteudo { get; set; } = string.Empty;

    /// <summary>Tamanho em bytes.</summary>
    public int Tamanho { get; set; }
}

/// <summary>Imagem na galeria da campanha, com o registro dono.</summary>
public class ImagemDaCampanhaResponseApiModel : ImagemResponseApiModel
{
    public int IdRegistro { get; set; }
    public string NomeRegistro { get; set; } = string.Empty;
    public int IdTipoRegistro { get; set; }
}
