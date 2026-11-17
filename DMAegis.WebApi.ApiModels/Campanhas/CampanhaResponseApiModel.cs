namespace DMAegis.WebApi.ApiModels.Campanhas;

public class CampanhaResponseApiModel
{
    public int Id { get; set; }
    public int IdSistema { get; set; }
    public string NomeSistema { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public bool Arquivada { get; set; }
    public DateTime DataCriacao { get; set; }
}
