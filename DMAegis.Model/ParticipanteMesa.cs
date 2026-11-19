namespace DMAegis.Model;

/// <summary>tb_participante_mesa: uma ficha no mapa e uma linha na iniciativa.</summary>
public class ParticipanteMesa
{
    /// <summary>co_seq_participante_mesa</summary>
    public int Id { get; set; }

    /// <summary>co_mesa</summary>
    public int IdMesa { get; set; }

    /// <summary>co_registro (opcional: ficha avulsa não tem)</summary>
    public int? IdRegistro { get; set; }

    /// <summary>no_participante</summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>st_jogador (personagem de jogador)</summary>
    public bool Jogador { get; set; }

    /// <summary>nu_iniciativa (nulo antes de rolar)</summary>
    public int? Iniciativa { get; set; }

    /// <summary>nu_bonus_iniciativa</summary>
    public int BonusIniciativa { get; set; }

    /// <summary>nu_ca</summary>
    public int? Ca { get; set; }

    /// <summary>nu_pv_atual</summary>
    public int? PvAtual { get; set; }

    /// <summary>nu_pv_maximo</summary>
    public int? PvMaximo { get; set; }

    /// <summary>nu_coluna (nulo = fora do mapa)</summary>
    public int? Coluna { get; set; }

    /// <summary>nu_linha (nulo = fora do mapa)</summary>
    public int? Linha { get; set; }

    /// <summary>nu_tamanho (1 a 4 células)</summary>
    public int Tamanho { get; set; } = 1;

    /// <summary>st_visivel (falso = oculta para os jogadores)</summary>
    public bool Visivel { get; set; } = true;

    /// <summary>nu_ordem (desempate manual e ordem de criação)</summary>
    public int Ordem { get; set; }
}
