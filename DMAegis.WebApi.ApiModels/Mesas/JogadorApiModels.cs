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

public class RolarRequestApiModel
{
    public string Expressao { get; set; } = string.Empty;
    public int Total { get; set; }
    public string Detalhe { get; set; } = string.Empty;

    /// <summary>Só vale para o mestre.</summary>
    public bool Secreta { get; set; }
}

/// <summary>Entrega aos jogadores (handout): texto e/ou imagem da galeria; destino nulo = todos.</summary>
public class InserirEntregaRequestApiModel
{
    public string Titulo { get; set; } = string.Empty;
    public string? Texto { get; set; }
    public int? IdImagemRegistro { get; set; }
    public int? IdRegistroDestino { get; set; }
}

public class EntregaResponseApiModel
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Texto { get; set; }
    public int? IdImagemRegistro { get; set; }
    public int? IdRegistroDestino { get; set; }
    public string? NomeDestino { get; set; }
    public DateTime Data { get; set; }
}

public class RolagemResponseApiModel
{
    public int Id { get; set; }
    public string Autor { get; set; } = string.Empty;
    public bool Mestre { get; set; }
    public string Expressao { get; set; } = string.Empty;
    public int Total { get; set; }
    public string Detalhe { get; set; } = string.Empty;
    public bool Secreta { get; set; }
    public DateTime Data { get; set; }
}
