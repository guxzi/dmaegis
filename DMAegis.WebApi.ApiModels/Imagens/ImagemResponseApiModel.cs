namespace DMAegis.WebApi.ApiModels.Imagens;

public class ImagemResponseApiModel
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string TipoConteudo { get; set; } = string.Empty;

    /// <summary>Tamanho em bytes.</summary>
    public int Tamanho { get; set; }
}
