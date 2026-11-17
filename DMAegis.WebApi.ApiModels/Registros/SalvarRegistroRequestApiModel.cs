namespace DMAegis.WebApi.ApiModels.Registros;

/// <summary>Usado para criar e para editar um registro. Atributos e tags são substituídos por inteiro.</summary>
public class SalvarRegistroRequestApiModel
{
    public int IdTipoRegistro { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Resumo { get; set; }
    public string? Descricao { get; set; }
    public List<AtributoApiModel> Atributos { get; set; } = [];
    public List<string> Tags { get; set; } = [];
}

public class AtributoApiModel
{
    public string Chave { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
}
