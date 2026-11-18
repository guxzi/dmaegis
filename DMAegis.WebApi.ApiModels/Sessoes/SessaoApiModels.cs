namespace DMAegis.WebApi.ApiModels.Sessoes;

public class InserirSessaoRequestApiModel
{
    /// <summary>Nulo = o maior número da campanha + 1.</summary>
    public int? Numero { get; set; }
    public DateOnly? Data { get; set; }
}

/// <summary>Preparo completo da sessão. Cenas e vínculos são substituídos por inteiro.</summary>
public class SalvarSessaoRequestApiModel
{
    public int Numero { get; set; }
    public DateOnly? Data { get; set; }
    public string? NotasPersonagens { get; set; }
    public string? ComecoForte { get; set; }
    public string? Resumo { get; set; }
    public List<string> Cenas { get; set; } = [];
    public List<int> IdsRegistro { get; set; } = [];
}

public class SalvarSegredoRequestApiModel
{
    public string Texto { get; set; } = string.Empty;
}

public class RevelarSegredoRequestApiModel
{
    public int IdSessao { get; set; }
}

public class SessaoResumoResponseApiModel
{
    public int Id { get; set; }
    public int Numero { get; set; }
    public DateOnly? Data { get; set; }
    public string? ComecoForte { get; set; }
}

public class SessaoResponseApiModel
{
    public int Id { get; set; }
    public int IdCampanha { get; set; }
    public int Numero { get; set; }
    public DateOnly? Data { get; set; }
    public string? NotasPersonagens { get; set; }
    public string? ComecoForte { get; set; }
    public string? Resumo { get; set; }
    public List<string> Cenas { get; set; } = [];
    public List<int> IdsRegistro { get; set; } = [];
    public List<SegredoResponseApiModel> Segredos { get; set; } = [];
}

public class SegredoResponseApiModel
{
    public int Id { get; set; }
    public string Texto { get; set; } = string.Empty;
    public int? IdSessaoCriacao { get; set; }
    public int? IdSessaoRevelacao { get; set; }
    public DateTime? DataRevelacao { get; set; }
}
