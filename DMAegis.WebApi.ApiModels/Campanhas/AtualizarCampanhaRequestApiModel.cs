namespace DMAegis.WebApi.ApiModels.Campanhas;

/// <summary>Sem IdSistema: o sistema não pode ser trocado depois de criada a campanha.</summary>
public class AtualizarCampanhaRequestApiModel
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
}
