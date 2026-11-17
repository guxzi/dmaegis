namespace DMAegis.WebApi.ApiModels.Registros;

public class RegistroResponseApiModel
{
    public int Id { get; set; }
    public int IdCampanha { get; set; }
    public int IdTipoRegistro { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Resumo { get; set; }
    public string? Descricao { get; set; }
    public List<AtributoApiModel> Atributos { get; set; } = [];
    public List<TagResponseApiModel> Tags { get; set; } = [];
    public DateTime DataCriacao { get; set; }
    public DateTime DataAtualizacao { get; set; }
}

public class RegistroResumoResponseApiModel
{
    public int Id { get; set; }
    public int IdTipoRegistro { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Resumo { get; set; }
    public List<TagResponseApiModel> Tags { get; set; } = [];
}

public class TagResponseApiModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}

public class TipoRegistroResponseApiModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}

public class ModeloAtributoResponseApiModel
{
    public int IdTipoRegistro { get; set; }
    public string Chave { get; set; } = string.Empty;
    public int Ordem { get; set; }
}
