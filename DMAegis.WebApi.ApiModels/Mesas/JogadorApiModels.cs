namespace DMAegis.WebApi.ApiModels.Mesas;

public class EntrarJogadorRequestApiModel
{
    public string Codigo { get; set; } = string.Empty;
}

public class JogadorResponseApiModel
{
    public string NomePersonagem { get; set; } = string.Empty;
    public string NomeCampanha { get; set; } = string.Empty;
}

public class MoverMinhaFichaRequestApiModel
{
    public int Coluna { get; set; }
    public int Linha { get; set; }
}

public class ConviteResponseApiModel
{
    public int IdRegistro { get; set; }
    public string NomePersonagem { get; set; } = string.Empty;
    public bool TemConvite { get; set; }
    public DateTime? DataUltimoAcesso { get; set; }
}

/// <summary>O código só aparece nesta resposta: o banco guarda o hash.</summary>
public class CodigoConviteResponseApiModel
{
    public int IdRegistro { get; set; }
    public string Codigo { get; set; } = string.Empty;
}
