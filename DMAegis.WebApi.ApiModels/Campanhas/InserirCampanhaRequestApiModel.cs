namespace DMAegis.WebApi.ApiModels.Campanhas;

public class InserirCampanhaRequestApiModel
{
    public int IdSistema { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
}
