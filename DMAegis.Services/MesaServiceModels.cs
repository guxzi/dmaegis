namespace DMAegis.Services;

// Modelos do MesaService (mesa, fichas, visão dos jogadores).

public class MesaResumoServiceModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int? IdImagemRegistro { get; set; }
    public bool Ativa { get; set; }
    public bool EmCombate { get; set; }
}

public class ParticipanteServiceModel
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

public class CelulaServiceModel
{
    public int Coluna { get; set; }
    public int Linha { get; set; }
}

public class MesaServiceModel
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
    public bool Nevoa { get; set; }
    public string? Clima { get; set; }
    public List<CelulaServiceModel> CelulasReveladas { get; set; } = [];
    public bool Ativa { get; set; }
    public bool EmCombate { get; set; }
    public int Rodada { get; set; }
    public int? IdParticipanteVez { get; set; }

    /// <summary>Na ordem da iniciativa (quem não tem iniciativa vai para o fim, na ordem de criação).</summary>
    public List<ParticipanteServiceModel> Participantes { get; set; } = [];
}

public class SalvarMesaServiceModel
{
    public string Nome { get; set; } = string.Empty;
    public int? IdImagemRegistro { get; set; }
    public int TamanhoCelula { get; set; } = 70;
    public int DeslocamentoX { get; set; }
    public int DeslocamentoY { get; set; }
    public bool GradeVisivel { get; set; } = true;
    public bool MovimentoLivre { get; set; } = true;
    public bool Nevoa { get; set; }
    public string? Clima { get; set; }
}

public class InserirParticipantesServiceModel
{
    /// <summary>Verdadeiro = todos os PJs da campanha que ainda não estão na mesa (ignora os outros campos).</summary>
    public bool TodosJogadores { get; set; }
    public int? IdRegistro { get; set; }

    /// <summary>Obrigatório para ficha avulsa; com registro, substitui o nome dele.</summary>
    public string? Nome { get; set; }
    public int Quantidade { get; set; } = 1;
}

public class SalvarParticipanteServiceModel
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

public class IniciativaServiceModel
{
    public int IdParticipante { get; set; }
    public int? Iniciativa { get; set; }
}

/// <summary>Ficha como o jogador a vê: sem PV nem CA de quem não é PJ.</summary>
public class ParticipanteVisaoServiceModel
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

    /// <summary>Só dos PJs (o jogador rola a própria iniciativa com o bônus).</summary>
    public int? Iniciativa { get; set; }
    public int? BonusIniciativa { get; set; }
}

public class VisaoJogadorServiceModel
{
    public int IdMesa { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int? IdImagemRegistro { get; set; }
    public int TamanhoCelula { get; set; }
    public int DeslocamentoX { get; set; }
    public int DeslocamentoY { get; set; }
    public bool GradeVisivel { get; set; }
    public bool MovimentoLivre { get; set; }
    public bool Nevoa { get; set; }
    public string? Clima { get; set; }
    public List<CelulaServiceModel> CelulasReveladas { get; set; } = [];
    public bool EmCombate { get; set; }
    public int Rodada { get; set; }
    public int? IdParticipanteVez { get; set; }
    public List<ParticipanteVisaoServiceModel> Participantes { get; set; } = [];
}
