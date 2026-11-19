namespace DMAegis.WebApi.ApiModels.Mesas;

public class SalvarMesaRequestApiModel
{
    public string Nome { get; set; } = string.Empty;
    public int? IdImagemRegistro { get; set; }
    public int TamanhoCelula { get; set; } = 70;
    public int DeslocamentoX { get; set; }
    public int DeslocamentoY { get; set; }
    public bool GradeVisivel { get; set; } = true;
    public bool MovimentoLivre { get; set; } = true;
}

/// <summary>Todos os PJs, um registro do caderno (com quantidade) ou uma ficha avulsa (só nome).</summary>
public class InserirParticipantesRequestApiModel
{
    public bool TodosJogadores { get; set; }
    public int? IdRegistro { get; set; }
    public string? Nome { get; set; }
    public int Quantidade { get; set; } = 1;
}

public class SalvarParticipanteRequestApiModel
{
    public string Nome { get; set; } = string.Empty;
    public int? Iniciativa { get; set; }
    public int BonusIniciativa { get; set; }
    public int? Ca { get; set; }
    public int? PvAtual { get; set; }
    public int? PvMaximo { get; set; }
    public int Tamanho { get; set; } = 1;
    public bool Visivel { get; set; } = true;
    public int Ordem { get; set; }
    public List<string> Condicoes { get; set; } = [];
}

/// <summary>Coluna e linha nulas = tirar a ficha do mapa.</summary>
public class MoverParticipanteRequestApiModel
{
    public int? Coluna { get; set; }
    public int? Linha { get; set; }
}

public class IniciativaRequestApiModel
{
    public int IdParticipante { get; set; }
    public int? Iniciativa { get; set; }
}

public class IniciarCombateRequestApiModel
{
    public List<IniciativaRequestApiModel> Iniciativas { get; set; } = [];
}

public class MesaResumoResponseApiModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int? IdImagemRegistro { get; set; }
    public bool Ativa { get; set; }
    public bool EmCombate { get; set; }
}

public class ParticipanteResponseApiModel
{
    public int Id { get; set; }
    public int? IdRegistro { get; set; }
    public int? IdTipoRegistro { get; set; }
    public int? IdImagemRetrato { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool Jogador { get; set; }
    public int? Iniciativa { get; set; }
    public int BonusIniciativa { get; set; }
    public int? Ca { get; set; }
    public int? PvAtual { get; set; }
    public int? PvMaximo { get; set; }
    public int? Coluna { get; set; }
    public int? Linha { get; set; }
    public int Tamanho { get; set; }
    public bool Visivel { get; set; }
    public int Ordem { get; set; }
    public List<string> Condicoes { get; set; } = [];
}

public class MesaResponseApiModel
{
    public int Id { get; set; }
    public int IdCampanha { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int? IdImagemRegistro { get; set; }
    public int TamanhoCelula { get; set; }
    public int DeslocamentoX { get; set; }
    public int DeslocamentoY { get; set; }
    public bool GradeVisivel { get; set; }
    public bool MovimentoLivre { get; set; }
    public bool Ativa { get; set; }
    public bool EmCombate { get; set; }
    public int Rodada { get; set; }
    public int? IdParticipanteVez { get; set; }
    public List<ParticipanteResponseApiModel> Participantes { get; set; } = [];
}

public class ParticipanteVisaoResponseApiModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool Jogador { get; set; }
    public int? IdTipoRegistro { get; set; }
    public int? IdImagemRetrato { get; set; }
    public int? Coluna { get; set; }
    public int? Linha { get; set; }
    public int Tamanho { get; set; }
    public int? Ca { get; set; }
    public int? PvAtual { get; set; }
    public int? PvMaximo { get; set; }
    public string? EstadoSaude { get; set; }
    public List<string> Condicoes { get; set; } = [];
    public bool EhMinha { get; set; }
}

public class VisaoJogadorResponseApiModel
{
    public int IdMesa { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int? IdImagemRegistro { get; set; }
    public int TamanhoCelula { get; set; }
    public int DeslocamentoX { get; set; }
    public int DeslocamentoY { get; set; }
    public bool GradeVisivel { get; set; }
    public bool MovimentoLivre { get; set; }
    public bool EmCombate { get; set; }
    public int Rodada { get; set; }
    public int? IdParticipanteVez { get; set; }
    public List<ParticipanteVisaoResponseApiModel> Participantes { get; set; } = [];
}
