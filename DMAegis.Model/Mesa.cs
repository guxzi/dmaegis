namespace DMAegis.Model;

/// <summary>tb_mesa: um mapa preparado da campanha, com a grade e o estado do combate.</summary>
public class Mesa
{
    /// <summary>co_seq_mesa</summary>
    public int Id { get; set; }

    /// <summary>co_campanha</summary>
    public int IdCampanha { get; set; }

    /// <summary>no_mesa</summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>co_imagem_registro (nulo = sem mapa, campo estrelado)</summary>
    public int? IdImagemRegistro { get; set; }

    /// <summary>nu_tamanho_celula (px da imagem)</summary>
    public int TamanhoCelula { get; set; } = 70;

    /// <summary>nu_deslocamento_x</summary>
    public int DeslocamentoX { get; set; }

    /// <summary>nu_deslocamento_y</summary>
    public int DeslocamentoY { get; set; }

    /// <summary>st_grade_visivel</summary>
    public bool GradeVisivel { get; set; } = true;

    /// <summary>st_movimento_livre (os jogadores podem mover a própria ficha)</summary>
    public bool MovimentoLivre { get; set; } = true;

    /// <summary>st_ativa (a que os jogadores veem; no máximo uma por campanha)</summary>
    public bool Ativa { get; set; }

    /// <summary>st_combate</summary>
    public bool EmCombate { get; set; }

    /// <summary>nu_rodada</summary>
    public int Rodada { get; set; }

    /// <summary>co_participante_vez</summary>
    public int? IdParticipanteVez { get; set; }

    /// <summary>dt_criacao</summary>
    public DateTime DataCriacao { get; set; }

    /// <summary>dt_atualizacao</summary>
    public DateTime DataAtualizacao { get; set; }
}
